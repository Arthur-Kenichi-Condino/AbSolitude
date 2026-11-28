using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Codice.LogWrapper;

namespace Unity.PlasticSCM.Editor.Diff.Asset.Diff.UnityObject
{
    internal enum MalformedHeaderReason
    {
        MissingHandle,
        EmptyType,
        MalformedType,
        MissingAnchor,
        MalformedAnchor,
        AnchorOutOfRange,
    }

    internal struct MalformedHeader
    {
        internal int LineNumber;
        internal MalformedHeaderReason Reason;
    }

    internal struct FileIdInfo
    {
        internal List<long> FileIds;
        internal Dictionary<long, long> HashByFileId;
        internal List<MalformedHeader> MalformedHeaders;
    }

    internal static class FileIdReader
    {
        internal static FileIdInfo Read(string yamlFilePath)
        {
            FileIdInfo result = new FileIdInfo
            {
                FileIds = new List<long>(),
                HashByFileId = new Dictionary<long, long>(),
                MalformedHeaders = new List<MalformedHeader>()
            };

            try
            {
                using (StreamReader reader = new StreamReader(yamlFilePath))
                {
                    string line;
                    int lineNumber = 0;
                    long currentFileId = 0;
                    long currentHash = 0;
                    bool inBlock = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        ++lineNumber;

                        if (line.StartsWith(DOCUMENT_START, StringComparison.Ordinal))
                        {
                            if (inBlock)
                                result.HashByFileId[currentFileId] = currentHash;

                            inBlock = false;
                            currentFileId = 0;
                            currentHash = 0;

                            if (IsLegacyHeader(line))
                                continue;

                            if (TryParseHeader(
                                line,
                                out string tag,
                                out long anchor,
                                out List<string> flags,
                                out MalformedHeaderReason reason))
                            {
                                result.FileIds.Add(anchor);
                                currentFileId = anchor;
                                currentHash = SeedBlockHash(tag, flags);
                                inBlock = true;
                            }
                            else
                            {
                                result.MalformedHeaders.Add(new MalformedHeader
                                {
                                    LineNumber = lineNumber,
                                    Reason = reason
                                });

                                mLog.WarnFormat(
                                    "Malformed subdocument header ({0}) at line {1} " +
                                    "of '{2}': \"{3}\"",
                                    ReasonText(reason), lineNumber, yamlFilePath, line);
                            }

                            continue;
                        }

                        if (inBlock)
                            currentHash = FoldHash(currentHash, line);
                    }

                    if (inBlock)
                        result.HashByFileId[currentFileId] = currentHash;
                }
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat(
                    "Error reading file IDs from '{0}': {1}",
                    yamlFilePath, ex.Message);
                mLog.DebugFormat(
                    "Stack trace:{0}{1}",
                    Environment.NewLine, ex.StackTrace);

                result.FileIds = new List<long>();
                result.HashByFileId = new Dictionary<long, long>();
                result.MalformedHeaders = new List<MalformedHeader>();
            }

            return result;
        }

        // Pre-5.2 files carry a "--- !u!header" document with no object. The engine skips it
        // without an error, so it is not reported. Only the exact type matches, not
        // "header-scheme:/...".
        static bool IsLegacyHeader(string line)
        {
            if (!line.StartsWith(LEGACY_HEADER, StringComparison.Ordinal))
                return false;

            return line.Length == LEGACY_HEADER.Length
                || IsHeaderWhitespace(line[LEGACY_HEADER.Length]);
        }

        // Called only once a line already matched DOCUMENT_START, so the tag, anchor and flags are
        // parsed in that order, the same order the engine's own header reader uses.
        static bool TryParseHeader(
            string line,
            out string tag,
            out long anchor,
            out List<string> flags,
            out MalformedHeaderReason reason)
        {
            tag = null;
            anchor = 0;
            flags = null;
            reason = default;

            // Every Unity header carries the !u! handle, so a header without it is malformed
            // rather than an unrecognised type.
            if (TAG_START + 2 >= line.Length || line[TAG_START + 1] != 'u' || line[TAG_START + 2] != '!')
            {
                reason = MalformedHeaderReason.MissingHandle;
                return false;
            }

            int typeStart = TAG_START + 3;
            int typeEnd = typeStart;
            while (typeEnd < line.Length && !IsHeaderWhitespace(line[typeEnd]))
                ++typeEnd;

            if (typeEnd == typeStart)
            {
                reason = MalformedHeaderReason.EmptyType;
                return false;
            }

            // YAML allows a '#' in a tag, but libyaml and PyYAML end the tag there and fail the
            // document, so no Unity file may carry one.
            if (line.IndexOf('#', typeStart, typeEnd - typeStart) >= 0)
            {
                reason = MalformedHeaderReason.MalformedType;
                return false;
            }

            tag = line.Substring(TAG_START, typeEnd - TAG_START);

            int anchorMarker = SkipWhitespace(line, typeEnd);
            if (anchorMarker >= line.Length || line[anchorMarker] != '&')
            {
                reason = MalformedHeaderReason.MissingAnchor;
                return false;
            }

            int anchorStart = anchorMarker + 1;
            int anchorEnd = anchorStart;
            if (anchorEnd < line.Length && line[anchorEnd] == '-')
                ++anchorEnd;

            int digitsStart = anchorEnd;
            while (anchorEnd < line.Length && IsDecimalDigit(line[anchorEnd]))
                ++anchorEnd;

            bool anchorWellFormed = anchorEnd > digitsStart
                && (anchorEnd == line.Length || IsHeaderWhitespace(line[anchorEnd]));
            if (!anchorWellFormed)
            {
                reason = MalformedHeaderReason.MalformedAnchor;
                return false;
            }

            string anchorText = line.Substring(anchorStart, anchorEnd - anchorStart);
            if (!long.TryParse(anchorText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out anchor))
            {
                reason = MalformedHeaderReason.AnchorOutOfRange;
                return false;
            }

            flags = new List<string>();
            int flagCursor = SkipWhitespace(line, anchorEnd);
            while (flagCursor < line.Length)
            {
                int flagStart = flagCursor;
                while (flagCursor < line.Length && !IsHeaderWhitespace(line[flagCursor]))
                    ++flagCursor;

                flags.Add(line.Substring(flagStart, flagCursor - flagStart));
                flagCursor = SkipWhitespace(line, flagCursor);
            }

            return true;
        }

        // The tag and flags carry the object's type and state, so a header change over an identical
        // body must still change the hash that decides whether the object can be skipped.
        static long SeedBlockHash(string tag, List<string> flags)
        {
            long hash = FoldHash(0, tag);
            for (int i = 0; i < flags.Count; ++i)
                hash = FoldHash(hash, flags[i]);
            return hash;
        }

        static long FoldHash(long hash, string text)
        {
            unchecked
            {
                return hash * 1099511628211 + text.GetHashCode();
            }
        }

        static bool IsHeaderWhitespace(char c)
        {
            return c == ' ' || c == '\t';
        }

        static bool IsDecimalDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        static int SkipWhitespace(string line, int offset)
        {
            while (offset < line.Length && IsHeaderWhitespace(line[offset]))
                ++offset;
            return offset;
        }

        static string ReasonText(MalformedHeaderReason reason)
        {
            switch (reason)
            {
                case MalformedHeaderReason.MissingHandle:
                    return "missing !u! handle";
                case MalformedHeaderReason.EmptyType:
                    return "empty type";
                case MalformedHeaderReason.MalformedType:
                    return "malformed type";
                case MalformedHeaderReason.MissingAnchor:
                    return "missing anchor";
                case MalformedHeaderReason.MalformedAnchor:
                    return "malformed anchor";
                case MalformedHeaderReason.AnchorOutOfRange:
                    return "anchor out of range";
                default:
                    return "unrecognised header";
            }
        }

        // Stops at the '!' rather than "--- !u!": a header without the handle must still be seen
        // and reported, not silently folded into the object before it.
        const string DOCUMENT_START = "--- !";

        const int TAG_START = 4; // index of the '!' within DOCUMENT_START

        const string LEGACY_HEADER = "--- !u!header";

        static readonly ILog mLog = PlasticApp.GetLogger("FileIdReader");
    }
}
