SELECT Bought.WishlistId, Wishlists.EventId, Wishlists.Name, Wishlists.Description, Wishlists.Link, Events.Name AS "eventName"
FROM Bought
INNER JOIN Wishlists
ON Bought.WishlistId = Wishlists.Id
INNER JOIN Events
ON Wishlists.EventId = Events.Id