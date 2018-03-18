using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Class for authorization related issues 
/// </summary>
public class Auth
{
    private Database db;
    private Events events;

    public int Register(string username, string password)
    {
        password = SHAEncrypt.GenerateSHA512String(password);
        using (db = Database.Open("aSpecialDay"))
        {
            int insert =  db.Execute("INSERT INTO Login (Username, Password) " +
                "SELECT @0, @1 " +
                "WHERE NOT EXISTS(SELECT Username FROM Login WHERE Username = @0)", username, password);

            return insert;
        }
    }

    /// <summary>
    /// Login the current user
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>Returns the number of rows</returns>
    public int Login(string username, string password)
    {
        password = SHAEncrypt.GenerateSHA512String(password);
        using (db = Database.Open("aSpecialDay"))
        {
            int loggedIn = db.QueryValue("SELECT COUNT(*) FROM Login WHERE Username = @0 AND Password = @1", username, password);
            return loggedIn;
        }
    }

    /// <summary>
    /// Get the Login.Id of the user
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public int GetUserId(string username)
    {
        int UserId;
        using (db = Database.Open("aSpecialDay"))
        {
            UserId = db.QueryValue("SELECT Id FROM Login WHERE Username = @0", username);
        }
        return UserId;
    }

    /// <summary>
    /// Insert guest with the secretkey
    /// </summary>
    /// <param name="username"></param>
    /// <param name="key"></param>
    /// <returns>Returns the affected rows</returns>
    public int InsertGuest(string username, string key)
    {
        int userId = GetUserId(username);
        events = new Events(0, username);
        int eventId = events.GetEventByKey(key);
        int insert = db.Execute("INSERT INTO Guests (UserId, EventId, Host) VALUES (@0, @1, 0) ", userId, eventId, key);
        return insert;
    }
}