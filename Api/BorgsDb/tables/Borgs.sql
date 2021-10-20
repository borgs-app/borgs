CREATE TABLE [dbo].[Borgs]
(
	[BorgId] INT NOT NULL PRIMARY KEY,
    [Url] NVARCHAR(MAX) NOT NULL,
    [Name] NVARCHAR(1000) NULL,
    [ParentId1] INT NULL,
    [ParentId2] INT NULL,
    [ChildId] INT NULL, 
    [DateAdded] DATETIME NOT NULL DEFAULT GETDATE()
    --CONSTRAINT [FK_Borgs_ToParentA] FOREIGN KEY ([ParentId1]) REFERENCES [Borgs]([BorgId]), 
    --CONSTRAINT [FK_Borgs_ToParentB] FOREIGN KEY ([ParentId2]) REFERENCES [Borgs]([BorgId]), 
    --CONSTRAINT [FK_Borgs_ToChild] FOREIGN KEY ([ChildId]) REFERENCES [Borgs]([BorgId])
)
