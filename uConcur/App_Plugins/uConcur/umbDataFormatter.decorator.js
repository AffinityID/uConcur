angular.module('umbraco.services').config(['$provide', function ($provide) {
    'use strict';
    $provide.decorator('umbDataFormatter', function ($delegate) {
        var baseFormatContentPostData = $delegate.formatContentPostData;
        $delegate.formatContentPostData = function(displayModel) {
            var formatted = baseFormatContentPostData.apply(this, arguments);
            formatted.metaData = {
                'uconcur:UpdateDate': displayModel.updateDate
            };
            return formatted;
        };
        return $delegate;
    });
}]);