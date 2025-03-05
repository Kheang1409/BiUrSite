using AutoMapper;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Newtonsoft.Json;

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

        public async Task<List<UserDto>> GetUsersAsync(int pageNumber, string usernamme)
        {
            var users = await _userRepository.GetUsersAsync(pageNumber, usernamme);
            return  _mapper.Map<List<UserDto>>(users);
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

        public async Task<bool> UserForgetPasswordAsync(string email, string opt){
            return await _userRepository.UserForgetPasswordAsync(email, opt);
        }
        public async Task<bool> UserResetPasswordAsync(string opt, string hashPassword){
            return await _userRepository.UserResetPasswordAsync(opt, hashPassword);
        }

        public async Task<bool> BanUserAsync(int userId){
            return await _userRepository.BanUserAsync(userId);
        }

        public async Task UpdateUserAsync(User User)
        {
            await _userRepository.UpdateUserAsync(User);
        }

        public async Task DeleteUserAsync(int UserId)
        {
            await _userRepository.DeleteUserAsync(UserId);
        }
    }
}