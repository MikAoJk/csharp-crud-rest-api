public class UserService(IDatabase database)
{
    public void SaveUser(UserRequest user) => database.SaveUser(user);

    public List<UserInDB> GetAllUsers() => database.GetAllUsers();

    public UserInDB? GetUserById(int id) => database.GetUserById(id);

    public bool UpdateUser(UserRequest user, int id) => database.UpdateUser(user, id);

    public bool DeleteUserById(int id) => database.DeleteUser(id);
}
