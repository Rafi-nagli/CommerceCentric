IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'StyleItemAttributes'))
BEGIN


	CREATE TABLE [dbo].[StyleItemAttributes](

	[Id][bigint] IDENTITY(1, 1) NOT NULL,

	[StyleItemId] [bigint] NOT NULL,

	[Name] [nvarchar](50) NULL,
		[Value] [ntext] NULL,
		[CreateDate] [datetime] NOT NULL,
		[CreatedBy] [bigint] NULL,
		[UpdateDate] [datetime] NULL,
		[UpdatedBy] [bigint] NULL,
	 CONSTRAINT[PK_StyleItemAttributes] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
	) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]
	
	ALTER TABLE [dbo].[StyleItemAttributes] ADD CONSTRAINT[DF_StyleItemAttributes_CreateDate]  DEFAULT (getdate()) FOR[CreateDate]	

END
GO
