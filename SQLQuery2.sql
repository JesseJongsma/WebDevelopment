﻿ALTER TABLE Wishlists
ADD FOREIGN KEY (Bought) REFERENCES Guests(UserId); 