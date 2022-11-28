using Enums;
using System;
using Models;
using Constants;
using MassTransit;
using Newtonsoft.Json;
using UserService.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace UserService.Consumers
{
	public class CreateUserConsumer : IConsumer<CreateUserModel>
	{
        private UserDbContext _dbContext;
        private IDistributedCache _distributedCache;

        public CreateUserConsumer(UserDbContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task Consume(ConsumeContext<CreateUserModel> context)
        {
            var userData = context.Message;
            var anUser = new User();
            anUser.Id = Guid.NewGuid();
            anUser.Name = userData.Name;
            anUser.CreatedDate = DateTime.Now;
            anUser.Status = UserStatus.Activated;

            // Save data to the database.
            _dbContext.Users.Add(anUser);
            await _dbContext.SaveChangesAsync();

            // Save data to the Redis.
            string jsonString = JsonConvert.SerializeObject(anUser);
            string redisKey = $"{RedisEntityKeys.USER_PREFIX}-{anUser.Id}";
            await _distributedCache.SetStringAsync(redisKey, jsonString);
        }
    }
}

