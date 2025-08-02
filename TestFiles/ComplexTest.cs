using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.Services
{
    /// <summary>
    /// Service for managing user accounts
    /// </summary>
    public class UserService : IUserService, IDisposable
    {
        private readonly ILogger<UserService> _logger;
        private readonly IDbContext _dbContext;
        private bool _disposed;

        public string ServiceName => "UserService";
        public int MaxRetries { get; set; } = 3;

        public UserService(ILogger<UserService> logger, IDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<User> GetUserAsync(int userId)
        {
            _logger.LogInformation("Getting user {UserId}", userId);
            
            var user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
                
            return user;
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbContext.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public interface IUserService
    {
        Task<User> GetUserAsync(int userId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
    }

    public interface IDbContext : IDisposable
    {
        IQueryable<User> Users { get; }
    }

    public interface ILogger<T>
    {
        void LogInformation(string message, params object[] args);
    }
}