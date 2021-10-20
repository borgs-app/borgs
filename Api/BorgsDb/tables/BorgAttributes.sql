CREATE TABLE [dbo].[BorgAttributes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[BorgId] INT NOT NULL,
	[AttributeId] INT NOT NULL, 
	[DateAdded] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_BorgAttributes_ToBorgs] FOREIGN KEY ([BorgId]) REFERENCES [Borgs]([BorgId]), 
    CONSTRAINT [FK_BorgAttributes_ToAttributes] FOREIGN KEY ([AttributeId]) REFERENCES [Attributes]([Id])
)
