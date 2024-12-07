import secrets
import string

def generate_secure_password(length=12):
    if length < 8:
        raise ValueError("Password length should be at least 8 characters.")

    # Define the characters that could be used in the password
    characters = string.ascii_letters + string.digits + string.punctuation

    # Generate a secure random string of characters of specified length
    password = ''.join(secrets.choice(characters) for i in range(length))

    return password