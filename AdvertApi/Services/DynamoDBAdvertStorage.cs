using AdvertApi.Models;
using System;
using System.Threading.Tasks;
using AutoMapper;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace AdvertApi.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;

        public DynamoDBAdvertStorage(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task<string>Add(AdvertModel model)
        {
            //throw new NotImplementedException();
            var dbModel = _mapper.Map<AdvertDbModel>(model);

            dbModel.Id = new Guid().ToString();
            dbModel.CreationDateTime = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var client = new AmazonDynamoDBClient())
            {
                using (var Context = new DynamoDBContext(client))
                {
                    await Context.SaveAsync(dbModel);
                }
            }
            return dbModel.Id;
        }

        public async Task<bool> Confirm(ConfirmAdvertModel model)
        {
            //throw new NotImplementedException();
            using(var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var record = await context.LoadAsync<AdvertDbModel>(model.Id);
                    if (record== null)
                    {
                        throw new KeyNotFoundException($"A record with ID = {model.Id} was not found.");
                    }
                    if(model.Status == AdvertStatus.Active)
                    {
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    }
                    else
                    {
                        await context.DeleteAsync(record);
                    }
                }
            }
            return true;
        }
    }
}
