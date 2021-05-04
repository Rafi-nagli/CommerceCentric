IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'CustomFeeds'))
BEGIN

	CREATE TABLE [dbo].[CustomFeeds](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[FeedName] [nvarchar](255) NULL,
		[DropShipperId] [bigint] NULL,
		[OverrideDSFeedType] [int] NULL,
		[OverrideDSProductType] [int] NULL,
		[Protocol] [nvarchar](50) NULL,
		[FtpSite] [nvarchar](512) NULL,
		[FtpFolder] [nvarchar](512) NULL,
		[UserName] [nvarchar](512) NULL,
		[Password] [nvarchar](512) NULL,
		[IsPassiveMode] [bit] NOT NULL,
		[IsSFTP] [bit] NOT NULL,
		[ExportFileType] [nvarchar](50) NULL,
		[ExportFileName] [nvarchar](255) NULL,
		[UpdateDate] [datetime] NULL,
		[CreateDate] [datetime] NOT NULL,
		[CreatedBy] [bigint] NULL,
	 CONSTRAINT [PK_DSFTPAccounts] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[CustomFeeds] ADD  CONSTRAINT [DF_DSFTPAccounts_IsPassiveMode]  DEFAULT ((1)) FOR [IsPassiveMode]

	ALTER TABLE [dbo].[CustomFeeds] ADD  CONSTRAINT [DF_DSFTPAccounts_IsSFTP]  DEFAULT ((0)) FOR [IsSFTP]

	ALTER TABLE [dbo].[CustomFeeds] ADD  CONSTRAINT [DF_DSFTPAccounts_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]

END
GO


IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'CustomFeedSchedules'))
BEGIN
	CREATE TABLE [dbo].[CustomFeedSchedules](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[CustomFeedId] [bigint] NOT NULL,
		[StartTime] [datetime] NOT NULL,
		[RecurrencyPeriod] [nvarchar](10) NULL,
		[RepeatInterval] [int] NULL,
		[DaysOfWeek] [nvarchar](50) NULL,
		[UpdateDate] [datetime] NULL,
		[CreateDate] [datetime] NOT NULL,
		[CreatedBy] [bigint] NULL,
	 CONSTRAINT [PK_CustomFeedSchedules] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[CustomFeedSchedules] ADD  CONSTRAINT [DF_CustomFeedSchedules_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
END
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'CustomFeedFields'))
BEGIN
	CREATE TABLE [dbo].[CustomFeedFields](
		[Id] [bigint] IDENTITY(1,1) NOT NULL,
		[CustomFeedId] [bigint] NOT NULL,
		[SourceFieldName] [nvarchar](255) NOT NULL,
		[CustomFieldName] [nvarchar](255) NULL,
		[CustomFieldValue] [nvarchar](255) NULL,
		[SortOrder] [int] NOT NULL,
		[CreateDate] [datetime] NOT NULL,
		[CreatedBy] [bigint] NULL,
	 CONSTRAINT [PK_CustomFeedFields] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[CustomFeedFields] ADD  CONSTRAINT [DF_CustomFeedFields_SortOrder]  DEFAULT ((0)) FOR [SortOrder]

	ALTER TABLE [dbo].[CustomFeedFields] ADD  CONSTRAINT [DF_CustomFeedFields_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
END
GO

