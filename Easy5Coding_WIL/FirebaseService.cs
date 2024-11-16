using Easy5Coding_WIL.Models;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Firebase.Database;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Firebase.Database.Query;

namespace Easy5Coding_WIL
{
    public class FirebaseService
    {
        private readonly FirebaseAuth _authProvider;
        private readonly FirebaseClient _firebaseClient;
        private readonly HttpClient _httpClient;
        private readonly string _firebaseApiKey;
        private readonly string _firebaseDatabaseUrl;
        private readonly ILogger<FirebaseService> _logger;

        // Constructor to initialize Firebase service with API key, database URL, and logger
        public FirebaseService(IHttpClientFactory httpClientFactory, string firebaseApiKey, string firebaseDatabaseUrl, ILogger<FirebaseService> logger)
        {
            _authProvider = FirebaseAuth.DefaultInstance;
            _httpClient = httpClientFactory.CreateClient();
            _firebaseApiKey = firebaseApiKey;
            _firebaseDatabaseUrl = firebaseDatabaseUrl;
            _logger = logger;

            var options = new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(firebaseApiKey)
            };

            _firebaseClient = new FirebaseClient(firebaseDatabaseUrl, options);
        }

        // Validate User Credentials using Firebase REST API
        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            try
            {
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_firebaseApiKey}";
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUri, content);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating user credentials: {ex.Message}");
                return false;
            }
        }

        // User Registration with Firebase Authentication and Role Assignment
        public async Task<bool> RegisterUserAsync(string email, string password, string role)
        {
            try
            {
                var userRecordArgs = new UserRecordArgs { Email = email, Password = password };
                var userRecord = await _authProvider.CreateUserAsync(userRecordArgs);

                // Store user role and employee details in Firebase Database
                var employeeDetails = new EmployeeDetails
                {
                    ContactEmail = email,
                    Role = role,
                    IsActive = true, // Set default active status
                    FirstName = email.Split('@')[0], // Example, you can modify this
                    LastName = "Default" // Example, you can modify this
                };

                await _firebaseClient
                    .Child("Users")
                    .Child(userRecord.Uid)
                    .PutAsync(new User
                    {
                        Uid = userRecord.Uid,
                        Email = email,
                        Password = password,
                        Role = role,
                        EmployeeDetails = employeeDetails,
                        LeaveRequests = new List<LeaveRequest>(), // Empty list for initial setup
                        Payroll = new Payroll() // Empty payroll for initial setup
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering user: {ex.Message}");
                return false;
            }
        }

        // Fetch user by email using Firebase Authentication
        public async Task<UserRecord> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _authProvider.GetUserByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user by email: {ex.Message}");
                return null;
            }
        }

        // Get User Role from Firebase Database
        public async Task<string> GetUserRoleAsync(string userId)
        {
            try
            {
                var user = await _firebaseClient
                    .Child("Users")
                    .Child(userId)
                    .Child("EmployeeDetails")
                    .Child("Role")
                    .OnceSingleAsync<string>();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving role for user {userId}: {ex.Message}");
                throw new Exception($"Error retrieving role for user {userId}: {ex.Message}");
            }
        }

        // Fetch all users (including employees) from Firebase
        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var usersSnapshot = await _firebaseClient.Child("Users").OnceAsync<User>();

                if (usersSnapshot == null || !usersSnapshot.Any())
                {
                    _logger.LogWarning("No users found in the database.");
                    return new List<User>();
                }

                var usersWithDetails = usersSnapshot
                    .Where(u => u.Object.EmployeeDetails != null)
                    .Select(u => u.Object)
                    .ToList();

                _logger.LogInformation($"Successfully retrieved {usersWithDetails.Count} users with employee details.");
                return usersWithDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving users: {ex.Message}");
                return new List<User>(); // Return an empty list to avoid breaking the application flow
            }
        }

        // Fetch an employee by ID from Firebase
        public async Task<User> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Error: User ID is null or empty.");
                return null;
            }

            try
            {
                var user = await _firebaseClient.Child("Users").Child(id).OnceSingleAsync<User>();

                if (user == null)
                {
                    _logger.LogWarning($"No user found with ID: {id}");
                    return null;
                }

                _logger.LogInformation($"Successfully retrieved user with ID: {id}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user by ID {id}: {ex.Message}");
                return null; // Return null to indicate failure
            }
        }


        // Update employee data in Firebase
        public async Task UpdateUserAsync(string id, User user)
        {
            try
            {
                await _firebaseClient.Child("Users").Child(id).PutAsync(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
            }
        }
        public async Task AddEmployeeAsync(EmployeeDetails employee)
        {
            try
            {
                // Get the User UID
                var userId = employee.EmployeeID; // Assuming EmployeeID is the UID of the logged-in user

                // Store employee details in Firebase
                await _firebaseClient
                    .Child("Users")
                    .Child(userId)
                    .Child("EmployeeDetails")  // Store under EmployeeDetails node
                    .PutAsync(employee);

                // Optionally, you can also handle other related data here (e.g., Payroll, LeaveRequests)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding employee: {ex.Message}");
                throw;  // Rethrow the exception for handling in the controller
            }
        }
        public async Task UpdateEmployeeAsync(string id, EmployeeDetails employee)
        {
            try
            {
                // Assuming EmployeeDetails is part of the User object in Firebase
                var user = await GetUserByIdAsync(id); // Retrieve the user
                if (user != null)
                {
                    user.EmployeeDetails = employee; // Update employee details
                    await UpdateUserAsync(id, user); // Use the existing UpdateUserAsync to save changes
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employee: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteEmployeeAsync(string id)
        {
            try
            {
                await _firebaseClient.Child("Users").Child(id).DeleteAsync(); // Deletes the user by ID
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employee: {ex.Message}");
                throw;
            }
        }

        // Fetch payroll records from Firebase
        public async Task<List<Payroll>> GetPayrollsAsync()
        {
            try
            {
                return (await _firebaseClient
                    
                    
                    .Child("Payrolls").OnceAsync<Payroll>())
                       .Select(p => p.Object).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving payrolls: {ex.Message}");
                return new List<Payroll>();
            }
        }

        // Add a new payroll record to Firebase
        public async Task AddPayrollAsync(Payroll payroll)
        {
            try
            {
                await _firebaseClient.Child("Payrolls").PostAsync(payroll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding payroll: {ex.Message}");
            }
        }

        // Fetch leave requests from Firebase
        public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
        {
            try
            {
                return (await _firebaseClient.Child("LeaveRequests").OnceAsync<LeaveRequest>())
                       .Select(lr => lr.Object).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving leave requests: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }
        public async Task<LeaveRequest> GetLeaveRequestByIdAsync(string id)
        {
            try
            {
                var leaveRequest = (await _firebaseClient.Child("LeaveRequests")
                                                        .OrderByKey() // You may need to order by key to search specifically for an ID
                                                        .EqualTo(id) // Assuming the ID is the key or part of the leave request
                                                        .OnceSingleAsync<LeaveRequest>());
                return leaveRequest;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving leave request by ID: {ex.Message}");
                return null;
            }
        }

        // Add a leave request to Firebase
        public async Task AddLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            try
            {
                await _firebaseClient.Child("LeaveRequests").PostAsync(leaveRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding leave request: {ex.Message}");
            }
        }
        public async Task<EmployeeDetails> GetEmployeeByIdAsync(string id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                return user?.EmployeeDetails;  // Return EmployeeDetails if found, otherwise null
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving employee by ID: {ex.Message}");
                return null;
            }
        }
        public async Task<Payroll> GetPayrollByIdAsync(string id)
        {
            try
            {
                var payroll = await _firebaseClient.Child("Payrolls").Child(id).OnceSingleAsync<Payroll>();
                return payroll;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving payroll by ID: {ex.Message}");
                return null;
            }
        }

        // Retrieve leave requests by EmployeeId
        public async Task<List<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(string employeeId)
        {
            try
            {
                return (await _firebaseClient.Child("Users")
                                              .Child(employeeId)
                                              .Child("LeaveRequests")
                                              .OnceAsync<LeaveRequest>())
                       .Select(lr => lr.Object).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving leave requests by employee ID: {ex.Message}");
                return new List<LeaveRequest>();
            }
        }
    }
}
