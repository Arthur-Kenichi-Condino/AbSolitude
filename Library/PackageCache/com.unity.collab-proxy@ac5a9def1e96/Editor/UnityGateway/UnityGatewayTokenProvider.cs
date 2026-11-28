using System;

using UnityEditor;

using Codice.Client.Common.Threading;
using Codice.LogWrapper;
using PlasticGui.UnityGateway;

namespace Unity.PlasticSCM.Editor.UnityGateway
{
    internal class UnityGatewayTokenProvider : IUnityGatewayTokenProvider
    {
        internal static Func<UnityCloudSettings> CloudSettingsFactory = () => new UnityCloudSettings()
        {
            AccessToken = CloudProjectSettings.accessToken
        };

        void IUnityGatewayTokenProvider.ExecuteForServiceToken(Action<string> onTokenReceived)
        {
            string genesisAccessToken = CloudSettingsFactory().AccessToken;

            if (string.IsNullOrEmpty(genesisAccessToken))
            {
                mLog.Error("No access token available in CloudProjectSettings");
                onTokenReceived(null);
                return;
            }

            string serviceAccessToken = null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter(50);
            waiter.Execute(
                /*threadOperationDelegate*/
                delegate
                {
                    if (!UnityGatewayServicesProvider.IsInitialized())
                    {
                        mLog.Error("UnityGatewayServicesProvider is not initialized");
                        return;
                    }

                    serviceAccessToken = UnityGatewayServicesProvider.GetAuthApi()
                        .ExchangeServiceAccessToken(genesisAccessToken);
                },
                /*afterOperationDelegate*/
                delegate
                {
                    if (waiter.Exception != null)
                        mLog.ErrorFormat("Could not obtain a service access token: {0}", waiter.Exception.Message);

                    onTokenReceived(serviceAccessToken);
                });
        }

        static readonly ILog mLog = PlasticApp.GetLogger("UnityGatewayTokenProvider");
    }
}
