namespace Tests.Shared.Mocks.Builders;

public class LoginRequestBuilder
{
    private string _userNameOrEmail = Constants.Users.Admin.UserName;
    private string _password = Constants.Users.Admin.Password;

    public LoginRequestBuilder WithUserNameOrEmail(string userNameOrEmail)
    {
        _userNameOrEmail = userNameOrEmail;
        return this;
    }

    public LoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public LoginUserRequestMock Build()
    {
        return new LoginUserRequestMock(_userNameOrEmail, _password);
    }
}
