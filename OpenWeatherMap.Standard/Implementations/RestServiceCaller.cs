﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenWeatherMap.Standard.Interfaces;
using OpenWeatherMap.Standard.Models;

namespace OpenWeatherMap.Standard.Implementations
{
    /// <summary>
    ///     "default" implementation of IRestService
    /// </summary>
    internal class RestServiceCaller : IRestService
    {
        private static HttpClient _httpClient;

        /// <summary>
        ///     the HttpClient to be used
        /// </summary>
        private static HttpClient HttpClient =>
            _httpClient ?? (_httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(10)});

        /// <summary>
        ///     get weather data and deserialize them
        /// </summary>
        /// <param name="url">the url to perform the request</param>
        /// <param name="iconDataBaseUrl">the root url to the weather icons</param>
        /// <param name="fetchIconData">whether to get icon-data from OWM-site or not</param>
        /// <returns><see cref="WeatherData"/> object</returns>
        public async Task<WeatherData> GetAsync(string url, string iconDataBaseUrl = "https://openweathermap.org/img/wn", bool fetchIconData = false)
        {
            try
            {
                var json = await HttpClient.GetStringAsync(url);
                var wd = JsonConvert.DeserializeObject<WeatherData>(json);

                if (fetchIconData)
                    await FetchIconDataAsync(wd, iconDataBaseUrl);

                return wd;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
#else
            catch
            {
                return null;
            }
#endif
        }

        /// <summary>
        ///     get forecast data and deserialize them
        /// </summary>
        /// <param name="url">the url to perform the request</param>
        /// <param name="iconDataBaseUrl">the base url to the icon's data</param>
        /// <param name="fetchIconData">whether to fetch icon's data or not</param>
        /// <returns><see cref="ForecastData" /> object</returns>
        public async Task<ForecastData> GetForecastAsync(string url, string iconDataBaseUrl = "https://openweathermap.org/img/wn", bool fetchIconData = false)
        {
            try
            {
                var json = await HttpClient.GetStringAsync(url);
                var fd = JsonConvert.DeserializeObject<ForecastData>(json);

                if (fetchIconData)
                    await FetchIconDataAsync(fd, iconDataBaseUrl);

                return fd;
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
#else
            catch
            {
                return null;
            }
#endif
        }

        /// <summary>
        ///     fetches the corresponding icon of the current weather-condition and adds it
        ///     to the <see cref="Weather.IconData" />-Property of the <see cref="WeatherData" />-array
        /// </summary>
        /// <param name="data">a filled <see cref="WeatherData" /> or <see cref="ForecastData"/> object</param>
        /// <param name="iconDataBaseUrl">the base-url where the images are stored</param>
        /// <returns></returns>
        private static async Task FetchIconDataAsync<T>(T data, string iconDataBaseUrl)
        {
            if (data is WeatherData weatherData)
            {
                foreach (var weather in weatherData.Weathers)
                    weather.IconData = await GetIconDataAsync($"{iconDataBaseUrl}/{weather.Icon}.png");
            }
            else if (data is ForecastData forecastData)
            {
                foreach (var weather in forecastData.WeatherData.SelectMany(a => a.Weathers))
                    weather.IconData = await GetIconDataAsync($"{iconDataBaseUrl}/{weather.Icon}.png");
            }
        }

        private static async Task<byte[]> GetIconDataAsync(string iconUrl)
        {
            try
            {
                return await HttpClient.GetByteArrayAsync(iconUrl);
            }
#if DEBUG
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
#else
            catch
            {
                return null;
            }
#endif
        }
    }
}