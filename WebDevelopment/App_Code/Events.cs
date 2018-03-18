using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for createWishlist
/// </summary>
public class Events
{
    private int _EventId;
    private string _Username;
    private Database db;
    private Auth auth;
    SHAEncrypt encrypt = new SHAEncrypt();

    /// <summary>
    /// Sets some of the data needed for the database Events
    /// </summary>
    /// <param name="EventId">Optional eventId</param>
    /// <param name="Username">Optional Username</param>
    public Events(int EventId = 0, string Username = null)
    {
        _EventId = EventId;
        _Username = Username;
        auth = new Auth();
    }

    /// <summary>
    /// Used to create a new event
    /// </summary>
    /// <param name="name">The name of the event</param>
    /// <param name="dateTime">The date and time of the event</param>
    /// <param name="description">The description of the event</param>
    /// <param name="location">The location of the event</param>
    /// <returns>returns the no of affected rows</returns>
    public int CreateEvent(string name, string dateTime, string description, string location)
    {
        string key = encrypt.RandomKey();
        dynamic selectLastAdded;
        using (db = Database.Open("aSpecialDay"))
        {
            string insertToEvents = "INSERT INTO Events (Name, DateTime, Description, Location, SecretKey) output INSERTED.Id as 'Id' VALUES (@0, @1, @2, @3, @4)";
            selectLastAdded = db.QuerySingle(insertToEvents, name, dateTime, description, location, key);
            //string selectLastAddedRow = "SELECT TOP 1 Id FROM Events ORDER BY Id DESC";
            if (selectLastAdded != null)
            {
                return db.Execute("INSERT INTO Guests (UserId, EventId, Host) VALUES (@0, @1, @2)", auth.GetUserId(_Username), selectLastAdded.Id, 1);
            }
        }

        return 0;
    }

    /// <summary>
    /// Used to update the event
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dateTime"></param>
    /// <param name="description"></param>
    /// <param name="location"></param>
    /// <returns>returns the number of affected rows</returns>
    public int UpdateEvent(string name, string dateTime, string description, string location)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            return db.Execute("UPDATE Events SET Name = @0, DateTime = @1, Description = @2 , Location = @3 " +
                    "WHERE Id = @4", name, Convert.ToDateTime(dateTime), description, location, _EventId);
        }
    }

    /// <summary>
    /// Removes everything where EventId exists
    /// </summary>
    /// <returns></returns>
    public int RemoveEvent()
    {
        using (db = Database.Open("aSpecialDay"))
        {
            db.Execute("DELETE FROM Guests WHERE EventId = @0", _EventId);
            return db.Execute("DELETE FROM Events WHERE Id = @0", _EventId);
        }
    }

    /// <summary>
    /// Used to get the EventId from secretkey
    /// </summary>
    /// <param name="key">The secret key of the Event table</param>
    /// <returns>
    /// Returns an integer of the EventKeu
    /// </returns>
    public int GetEventByKey(string key)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            int eventId = db.QueryValue("SELECT Id FROM Events WHERE SecretKey = @0", key);
            return eventId;
        }
    }

    /// <summary>
    /// Returns a row from events by the eventId
    /// </summary>
    /// <param name="id"></param>
    /// <returns>returns a row</returns>
    public dynamic GetEventById(int id)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            dynamic selectEvent = db.QuerySingle("SELECT * FROM Events WHERE Id = @0", id);
            return selectEvent;
        }
    }

    /// <summary>
    /// Used to get all the events of a specific user
    /// </summary>
    /// <returns>Returns the whole event table where the user is guest or host</returns>
    public IEnumerable<dynamic> GetAllMyEvents()
    {
        int userId = auth.GetUserId(_Username);
        using (db = Database.Open("aSpecialDay"))
        {
            dynamic result = db.Query("SELECT * " +
            "FROM Guests " +
            "FULL OUTER JOIN Events ON Events.Id = Guests.EventId " +
            "WHERE UserId = @0", userId);

            return result;
        }
    }

    /// <summary>
    /// Get title for the event(Required the class eventId to be set)
    /// </summary>
    /// <returns>Returns the name column of the event</returns>
    public string GetTitle()
    {
        dynamic selectTitle;
        using (db = Database.Open("aSpecialDay"))
        {
            selectTitle = db.QueryValue("SELECT Name FROM Events WHERE Id = @0", _EventId);
        }
        return selectTitle.ToString();
    }

    /// <summary>
    /// Checks if the user is indeed the host of the event(Required the class eventId to be set)
    /// </summary>
    /// <returns>Returns a true or false</returns>
    public bool GetHost(string userId = "")
    {
        dynamic selectHost;
        using (db = Database.Open("aSpecialDay"))
        {
            if (userId == "")
            {
                selectHost = db.QueryValue("SELECT Host FROM Guests WHERE UserId = @0 AND EventId = @1", auth.GetUserId(_Username), _EventId);
            }
            else
            {
                selectHost = db.QueryValue("SELECT Host FROM Guests WHERE UserId = @0 AND EventId = @1", userId, _EventId);
            }
        }
        if (selectHost == null)
            return false;
        return selectHost;
    }

    /// <summary>
    /// Check if the user is a guest or host
    /// </summary>
    /// <returns>returns the row of the user</returns>
    public dynamic GetGuest()
    {
        dynamic selectGuest;
        using (db = Database.Open("aSpecialDay"))
        {
            selectGuest = db.QuerySingle("SELECT * FROM Guests WHERE UserId = @0 AND EventId = @1", auth.GetUserId(_Username), _EventId);
        }
        if (selectGuest == null)
            return null;
        return selectGuest;
    }

    /// <summary>
    /// Returns all the guests from the given event
    /// </summary>
    /// <returns>Returns dynamic</returns>
    public IEnumerable<dynamic> GetGuests()
    {
        dynamic result;
        using (Database db = Database.Open("aSpecialDay"))
        {
            return result = db.Query("SELECT * " +
                "FROM Guests " +
                "JOIN Login ON Login.Id = Guests.UserId " +
                "WHERE Guests.EventId = @0", _EventId);
        }
    }

    /// <summary>
    /// Used to change host to guest or guest to host
    /// </summary>
    /// <param name="userId">UserId from the form</param>
    /// <returns>returns the affected row</returns>
    public int UpdateGuests(string userId)
    {
        bool host = GetHost(userId);
        using (db = Database.Open("aSpecialDay"))
        {
            int lastHost = db.QueryValue("SELECT COUNT(*) AS 'count' FROM Guests WHERE EventId = @0 AND Host = 1", _EventId);
            if (lastHost > 1 || !host)
            {
                return db.Execute("UPDATE Guests SET Host = @0 WHERE UserId = @1 AND EventId = @2", !host, userId, _EventId);
            }
        }
        return 0;
    }
}