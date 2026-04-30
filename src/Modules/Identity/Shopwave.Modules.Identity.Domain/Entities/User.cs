using Shopwave.Shared.Domain;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Shopwave.Modules.Identity.Domain.Enums;
using Shopwave.Modules.Identity.Domain.Events;

namespace Shopwave.Modules.Identity.Domain.Entities;

/// <summary>
/// Represents a user in the identity domain.
/// A user is an aggregate root that encapsulates user information and behavior.
/// </summary>
public class User : AggregateRoot
{
    private string _firstName = default!;
    private string _lastName = default!;
    private string _email = default!;
    private string _phoneNumber = default!;
    private string _passwordHash = default!;
    private UserRole _role;

    private static readonly Regex NameRegex = new(
        @"^[\p{L}'-]+$",
        RegexOptions.Compiled
    );
    
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{7,14}$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is empty or contains invalid characters.</exception>
    public string FirstName
    {
        get => _firstName;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("First name cannot be empty.", nameof(value));

            if (!NameRegex.IsMatch(value))
                throw new ArgumentException("First name contains invalid characters.", nameof(value));

            _firstName = value;
        }
    }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is empty or contains invalid characters.</exception>
    public string LastName
    {
        get => _lastName;
        protected set
        {

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Last name cannot be empty.", nameof(value));

            if (!NameRegex.IsMatch(value))
                throw new ArgumentException("Last name contains invalid characters.", nameof(value));

            _lastName = value;
        }
    }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the email is empty or has an invalid format.</exception>
    public string Email
    {
        get => _email;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("email cannot be empty.", nameof(value));

            value = value.Trim();

            try
            {
                var addr = new MailAddress(value);

                if (addr.Address != value)
                    throw new ArgumentException("invalid email format.", nameof(value));

                _email = value.ToLowerInvariant();
            }
            catch
            {
                throw new ArgumentException("invalid email format.", nameof(value));
            }
        }
    }

    /// <summary>
    /// Gets the hashed password for the user.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the password hash is empty.</exception>
    public string PasswordHash
    {
        get => _passwordHash;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("password hash cannot be empty.", nameof(value));

            _passwordHash = value;
        }
    }

    /// <summary>
    /// Gets the user's phone number.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the phone number is empty or has an invalid format.</exception>
    public string PhoneNumber
    {
        get => _phoneNumber;
        protected set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("phone number cannot be empty.", nameof(value));

            value = value.Trim();

            if (!PhoneRegex.IsMatch(value))
                throw new ArgumentException("invalid phone number format.", nameof(value));

            _phoneNumber = value;
        }
    }
    
    /// <summary>
    /// Gets the user's role in the system.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the role is not a defined UserRole value.</exception>
    public UserRole Role
    {
        get => _role;
        protected set
        {
            if (!Enum.IsDefined(typeof(UserRole), value))
                throw new ArgumentException("invalid user role.", nameof(value));

            _role = value;
        }
    }
    
    private User()
    {}

    /// <summary>
    /// Creates a new instance of the <see cref="User"/> class with the specified details.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="passwordHash">The hashed password for the user.</param>
    /// <param name="phoneNumber">The user's phone number.</param>
    /// <param name="role">The user's role in the system.</param>
    /// <returns>A new <see cref="User"/> instance with the specified details.</returns>
    /// <exception cref="ArgumentException">Thrown when any parameter validation fails.</exception>
    public static User Create(string firstName, string lastName, string email, string passwordHash, string 
            phoneNumber, UserRole role)
    {
        var user = new User();
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.PasswordHash = passwordHash;
        user.PhoneNumber = phoneNumber;
        user.Role = role;
        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.Email));

        return user;
    }

    /// <summary>
    /// Soft-deletes the user by marking it as deleted and recording the deletion time.
    /// Raises a <see cref="UserDeletedEvent"/> domain event.
    /// </summary>
    public void Delete()
    {
        this.IsDeleted = true;
        this.DeletedAt = DateTime.UtcNow;

        this.RaiseDomainEvent(new UserDeletedEvent(this.Id, this.Email));
    }
}