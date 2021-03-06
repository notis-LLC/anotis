﻿namespace Anotis.Models.Attendance
{
    public class UrlResolver
    {
        private readonly AnotisConfig _config;

        public UrlResolver(AnotisConfig config)
        {
            _config = config;
        }

        public string UrlString(long userId)
        {
            return string.Format(_config.Shikimori.AuthLinkTemplate, _config.Shikimori.RedirectUrl, userId);
        }
    }
}