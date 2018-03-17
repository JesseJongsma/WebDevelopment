SELECT *
FROM Hosts 
FULL OUTER JOIN Events ON Events.Id = Hosts.EventId
WHERE UserId = 1