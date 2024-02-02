angular.module("umbraco").controller("Cogflare.Controller", function (notificationsService, cogflareResources) {

    var vm = this;

    vm.dashboard = {
        isPurging: false,
        setError: console.error,
        isCogFlareEnabled: false,
        keyNodes: "",
        endpoint: "",
        email: "",
        apiKey:"",
        isShowSettingsEnabled: false
    };
    vm.getSettings = function () {

        cogflareResources.getSettings()
            .then(
                function (response) {
                    vm.isShowSettingsEnabled = true;
                    vm.keyNodes = response.data.KeyNodes;
                    vm.isCogFlareEnabled = response.data.IsEnabled;
                    vm.endpoint = response.data.Endpoint;
                    vm.email = response.data.Email;
                    vm.apiKey = response.data.ApiKey;
                }
            );
    };

    vm.purgeCache = function () {

        vm.isPurging = true;

        cogflareResources.purgeCache()
            .then(
                function (response) {
                    if (response.data === true)
                        notificationsService.success("CogFlare", "CloudFlare purged successfully! Check logs for more details");
                    else
                        notificationsService.error("CogFlare", "Something went wrong. Possible that not all cache was purged. Check logs for more details");

                    vm.dashboard = response;
                },
                function (error) {
                    vm.dashboard.setError(error);
                    notificationsService.error("CogFlare", "Something went wrong. Possible that not all cache was purged. Check logs for more details");
                }
            )
            .then(function () {
                vm.isPurging = false;
            });
    };
});