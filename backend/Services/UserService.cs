using AutoMapper;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<User>> GetUsersAsync(int pageNumber, string? usernamme)
        {
            var users = await _userRepository.GetUsersAsync(pageNumber, usernamme);
            return users;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {  
            var user = await _userRepository.GetUserByIdAsync(userId);
            return _mapper.Map<User>(user);
        }

        public async Task<User?> GetUserByEmailAsync(string email){
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task AddUserAsync(User User)
        {
            await _userRepository.AddUserAsync(User);
        }

        public async Task<bool> UserVerifiedAsync(string verifiedToken){
            return await _userRepository.UserVerifiedAsync(verifiedToken);
        }

        public async Task<bool> UserForgetPasswordAsync(string email, string? otp){
            return await _userRepository.UserForgetPasswordAsync(email, otp);
        }
        public async Task<bool> UserResetPasswordAsync(string otp, string hashPassword){
            return await _userRepository.UserResetPasswordAsync(otp, hashPassword);
        }

        public async Task<bool> BanUserAsync(int userId){
            return await _userRepository.BanUserAsync(userId);
        }

        public async Task UpdateUserAsync(User User)
        {
            await _userRepository.UpdateUserAsync(User);
        }

        public async Task<bool> SoftDeleteUserAsync(int userId){
            return await _userRepository.SoftDeleteUserAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int UserId)
        {
            return await _userRepository.DeleteUserAsync(UserId);
        }
    }
}