﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebMatrix.Data;

/// <summary>
/// Summary description for Wishlists
/// </summary>
public class Wishlists
{
    Auth auth = new Auth();
    Events events;
    Database db;
    private string _Username;
    private int _EventId;

    /// <summary>
    /// The constructor of this class
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="username"></param>
    public Wishlists(int eventId = 0, string username = "")
    {
        _Username = username;
        _EventId = eventId;
    }

    /// <summary>
    /// Inserts a new wishlist item for an event
    /// </summary>
    /// <param name="name">name of the gift</param>
    /// <param name="description">description of the gift</param>
    /// <param name="link">link to the gift</param>
    /// 
    public void InsertWishlist(string name, string description, string link)
    {
        events = new Events(_EventId, _Username);
        if (events.GetHost())
        {
            using (db = Database.Open("aSpecialDay"))
            {
                db.Execute("INSERT INTO Wishlists (EventId, Name, Description, Link) " +
                    "VALUES (@0, @1, @2, @3)", _EventId, name, description, link);
            }
        }
    }

    /// <summary>
    /// Update the wishlist
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="link"></param>
    /// <param name="wishlistId"></param>
    /// <returns>Returns number of rows affected</returns>
    public int Updatewishlist(string name, string description, string link, int wishlistId)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            return db.Execute("UPDATE Wishlists SET Name = @0, Description = @1, Link = @2 WHERE Id = @3", name, description, link, wishlistId);
        }
    }

    public int RemoveWishlist(int wishlistId)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            return db.Execute("DELETE FROM Wishlists WHERE Id = @0", wishlistId);
        }
    }

    /// <summary>
    /// Used to retrieve the wishlist for the given event(Required the class eventId to be set)
    /// </summary>
    /// <returns>Returns the whole table of Wishlists where eventid equals the class variable eventId</returns>
    public IEnumerable<dynamic> GetWishlist()
    {
        IEnumerable<dynamic> result = null;
        using (db = Database.Open("aSpecialDay"))
        {
            result = db.Query("SELECT * FROM Wishlists WHERE EventId = @0", _EventId);
        }
        return result;
    }

    /// <summary>
    /// Checks if the wishlist item is already bought or not
    /// </summary>
    /// <param name="wishlistId">The id of the wishlist item</param>
    /// <returns>Returns a true if bought</returns>
    public bool CheckBought(int wishlistId)
    {
        dynamic result;
        using (db = Database.Open("aSpecialDay"))
        {
            result = db.Query("SELECT * FROM Bought WHERE WishlistId = @0", wishlistId);
        }

        if (result.Count > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Let a user buy an item
    /// </summary>
    /// <param name="wishlistId"></param>
    /// <returns></returns>
    public int BuyItem(int wishlistId)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            return db.Execute("INSERT INTO Bought (LoginId, WishlistId) " +
                        "SELECT @0, @1 " +
                        "WHERE NOT EXISTS(" +
                        "SELECT * FROM Bought WHERE LoginId = @0 AND WishlistId = @1) ", auth.GetUserId(_Username), Convert.ToInt32(wishlistId));
        }
    }

    /// <summary>
    /// Return the item back from bought state
    /// </summary>
    /// <param name="wishlistId"></param>
    /// <returns></returns>
    public int ReturnItem(int wishlistId)
    {
        using (db = Database.Open("aSpecialDay"))
        {
            var selectBought = db.Query("SELECT WishlistId FROM Bought WHERE LoginId = @0", auth.GetUserId(_Username));
            if (selectBought != null)
            {
                return db.Execute("DELETE FROM Bought WHERE LoginId = @0 AND WishlistId = @1", auth.GetUserId(_Username), wishlistId);
            }
        }
        return 0;
    }

    /// <summary>
    /// Load the boughtlist
    /// </summary>
    /// <returns></returns>
    public IEnumerable<dynamic> GetBoughtList()
    {
        using (db = Database.Open("aSpecialDay"))
        {
            var selectBought = db.Query("SELECT WishlistId FROM Bought WHERE LoginId = @0", auth.GetUserId(_Username));
            if (selectBought != null)
            {
                return db.Query("" +
                    "SELECT Bought.WishlistId, Wishlists.EventId, Wishlists.Name, Wishlists.Description, Wishlists.Link, Events.Name AS 'eventName' " +
                    "FROM Bought " +
                    "INNER JOIN Wishlists " +
                    "ON Bought.WishlistId = Wishlists.Id " +
                    "INNER JOIN Events " +
                    "ON Wishlists.EventId = Events.Id " +
                    "WHERE Bought.LoginId = @0", auth.GetUserId(_Username));
            }
            return null;
        }
    }
}