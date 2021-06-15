// <copyright file="SimpleCalculatorController.cs" company="APIMatic">
// Copyright (c) APIMatic. All rights reserved.
// </copyright>
namespace SimpleCalculator.Standard.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Converters;
    using SimpleCalculator.Standard;
    using SimpleCalculator.Standard.Authentication;
    using SimpleCalculator.Standard.Http.Client;
    using SimpleCalculator.Standard.Http.Request;
    using SimpleCalculator.Standard.Http.Response;
    using SimpleCalculator.Standard.Utilities;

    /// <summary>
    /// SimpleCalculatorController.
    /// </summary>
    public class SimpleCalculatorController : BaseController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCalculatorController"/> class.
        /// </summary>
        /// <param name="config"> config instance. </param>
        /// <param name="httpClient"> httpClient. </param>
        /// <param name="authManagers"> authManager. </param>
        internal SimpleCalculatorController(IConfiguration config, IHttpClient httpClient, IDictionary<string, IAuthManager> authManagers)
            : base(config, httpClient, authManagers)
        {
        }

        /// <summary>
        /// Calculates the expression using specified operation..
        /// </summary>
        /// <param name="operation">Required parameter: The operator to apply on the variables.</param>
        /// <param name="x">Required parameter: This is LHS value.</param>
        /// <param name="y">Required parameter: This is RHS value.</param>
        /// <returns>Returns the double response from the API call.</returns>
        public double Calculate(
                Models.OperationTypeEnum operation,
                double x,
                double y)
        {
            Task<double> t = this.CalculateAsync(operation, x, y);
            ApiHelper.RunTaskSynchronously(t);
            return t.Result;
        }

        /// <summary>
        /// Calculates the expression using specified operation..
        /// </summary>
        /// <param name="operation">Required parameter: The operator to apply on the variables.</param>
        /// <param name="x">Required parameter: This is LHS value.</param>
        /// <param name="y">Required parameter: This is RHS value.</param>
        /// <param name="cancellationToken"> cancellationToken. </param>
        /// <returns>Returns the double response from the API call.</returns>
        public async Task<double> CalculateAsync(
                Models.OperationTypeEnum operation,
                double x,
                double y,
                CancellationToken cancellationToken = default)
        {
            // the base uri for api requests.
            string baseUri = this.Config.GetBaseUri();

            // prepare query string for API call.
            StringBuilder queryBuilder = new StringBuilder(baseUri);
            queryBuilder.Append("/{operation}");

            // process optional template parameters.
            ApiHelper.AppendUrlWithTemplateParameters(queryBuilder, new Dictionary<string, object>()
            {
                { "operation", ApiHelper.JsonSerialize(operation).Trim('\"') },
            });

            // prepare specfied query parameters.
            var queryParams = new Dictionary<string, object>()
            {
                { "x", x },
                { "y", y },
            };

            // append request with appropriate headers and parameters
            var headers = new Dictionary<string, string>()
            {
                { "user-agent", this.UserAgent },
            };

            // prepare the API call request to fetch the response.
            HttpRequest httpRequest = this.GetClientInstance().Get(queryBuilder.ToString(), headers, queryParameters: queryParams);

            // invoke request and get response.
            HttpStringResponse response = await this.GetClientInstance().ExecuteAsStringAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            HttpContext context = new HttpContext(httpRequest, response);

            // handle errors defined at the API level.
            this.ValidateResponse(response, context);

            return double.Parse(response.Body);
        }
    }
}