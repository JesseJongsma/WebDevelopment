using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for createWishlist
/// </summary>
public class CreateWishlist
{
    private int _EventId;
    private string _Username;
    private Database db;

    public CreateWishlist(int EventId = 0, string Username = null)
    {
        _EventId = EventId;
        _Username = Username;
        db = Database.Open("aSpecialDay");
    }

    public void InsertData(string name, string description, string link)
    {
        if (CheckEventForUser())
        {
            using (db)
            {
                db.Execute("INSERT INTO Wishlists (EventId, Name, Description, Link) " +
                    "VALUES (@0, @1, @2, @3)", _EventId, name, description, link);
            }
        }
    }

    public int GetUserId()
    {
        int UserId;
        using (db)
        {
            UserId = db.QueryValue("SELECT Id FROM Login WHERE Username = @0", _Username);
        }
        return UserId;
    }

    /// <summary>
    /// This method checks if the user is authorised to modify an event.
    /// </summary>
    /// <returns>
    /// Returns true if the user is host or guest to an event.
    /// </returns>
    public bool CheckEventForUser()
    {
        dynamic result = null;
        using (db)
        {
            result = db.QuerySingle("SELECT * FROM Guests WHERE UserId = @0 AND EventId = @1", GetUserId(), _EventId);
        }

        if (result != null)
        {
            return true;
        }

        return false;
    }

    public string GetTitle()
    {
        dynamic selectTitle;
        using (db)
        {
            selectTitle = db.QueryValue("SELECT Name FROM Events WHERE Id = @0", _EventId);
        }
        return selectTitle.ToString();
    }

    public bool GetHost()
    {
        dynamic selectHost;
        using (db)
        {
            selectHost = db.QueryValue("SELECT Host FROM Guests WHERE UserId = @0 AND EventId = @1", GetUserId(), _EventId);
        }
        return selectHost;
    }

    public IEnumerable<dynamic> GetWishlist()
    {
        IEnumerable<dynamic> result = null;
        using (db)
        {
            result = db.Query("SELECT * FROM Wishlists WHERE EventId = @0", _EventId);
        }
        return result;
    }

    public bool CheckBought(int wishlistId)
    {
        dynamic result;
        using (db)
        {
            result = db.Query("SELECT * FROM Bought WHERE WishlistId = @0", wishlistId);
        }

        if (result.Count > 0)
        {
            return true;
        }
        return false;
    }
}