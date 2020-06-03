using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Database;
using Anotis.Models.SourceWorkers;
using Microsoft.Extensions.Logging;
using ShikimoriSharp;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Enums;
using ShikimoriSharp.Settings;

namespace Anotis.Models
{
    public class NewUserModel : IControllerService
    {
        private readonly IDatabase _database;
        private readonly IQueueProviderService _service;

        public NewUserModel(IDatabase database, IQueueProviderService service)
        {
            _database = database;
            _service = service;
        }

        public async Task NewUser(Sources source, string code, long state)
        {
            ISourceWorker worker = source switch
            {
                Sources.Shikimori => new ShikimoriWorker(),
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
            
            var user = new User
            {
                Animes = new List<AnimeID>(),
                Mangas = new List<MangaID>(),
                Token = await worker.GenerateToken(code),
                UserCredentials = new Credential(state, Sources.Shikimori)
            };
            user.UserDbId = _database.InsertUser(user);
        }

        private async Task RefreshNewUser(ISourceWorker worker, long databaseId)
        {
            var value = _database.GetById(databaseId);
            var getUser = await worker.GetUser(value.Token);
            getUser.UserCredentials = value.UserCredentials;
            _database.UpdateUser(databaseId, getUser);
        }

        
    }
}