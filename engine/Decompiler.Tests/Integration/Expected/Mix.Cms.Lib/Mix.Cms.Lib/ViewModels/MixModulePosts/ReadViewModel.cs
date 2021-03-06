using Microsoft.EntityFrameworkCore.Storage;
using Mix.Cms.Lib;
using Mix.Cms.Lib.Models.Cms;
using Mix.Cms.Lib.ViewModels.MixModules;
using Mix.Cms.Lib.ViewModels.MixPosts;
using Mix.Domain.Core.ViewModels;
using Mix.Domain.Data.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Mix.Cms.Lib.ViewModels.MixModulePosts
{
	public class ReadViewModel : ViewModelBase<MixCmsContext, MixModulePost, Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel>
	{
		[JsonProperty("createdBy")]
		public string CreatedBy
		{
			get;
			set;
		}

		[JsonProperty("createdDateTime")]
		public DateTime CreatedDateTime
		{
			get;
			set;
		}

		[JsonProperty("description")]
		public string Description
		{
			get;
			set;
		}

		[JsonProperty("id")]
		public int Id
		{
			get;
			set;
		}

		[JsonProperty("image")]
		public string Image
		{
			get;
			set;
		}

		[JsonProperty("isActived")]
		public bool IsActived
		{
			get;
			set;
		}

		[JsonProperty("lastModified")]
		public DateTime? LastModified
		{
			get;
			set;
		}

		[JsonProperty("modifiedBy")]
		public string ModifiedBy
		{
			get;
			set;
		}

		[JsonProperty("module")]
		public Mix.Cms.Lib.ViewModels.MixModules.ReadListItemViewModel Module
		{
			get;
			set;
		}

		[JsonProperty("moduleId")]
		public int ModuleId
		{
			get;
			set;
		}

		[JsonProperty("post")]
		public Mix.Cms.Lib.ViewModels.MixPosts.ReadListItemViewModel Post
		{
			get;
			set;
		}

		[JsonProperty("postId")]
		public int PostId
		{
			get;
			set;
		}

		[JsonProperty("priority")]
		public int Priority
		{
			get;
			set;
		}

		[JsonProperty("specificulture")]
		public string Specificulture
		{
			get;
			set;
		}

		[JsonProperty("status")]
		public MixEnums.MixContentStatus Status
		{
			get;
			set;
		}

		public ReadViewModel(MixModulePost model, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			base(model, _context, _transaction);
			return;
		}

		public ReadViewModel()
		{
			base();
			return;
		}

		public override void ExpandView(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			stackVariable0 = ViewModelBase<MixCmsContext, MixPost, Mix.Cms.Lib.ViewModels.MixPosts.ReadListItemViewModel>.Repository;
			V_2 = Expression.Parameter(Type.GetTypeFromHandle(// 
			// Current member / type: System.Void Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel::ExpandView(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
			// Exception in: System.Void ExpandView(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
			// Specified method is not supported.
			// 
			// mailto: JustDecompilePublicFeedback@telerik.com


		public static RepositoryResponse<List<Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel>> GetModulePostNavAsync(int postId, string specificulture, MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			V_0 = new Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel.u003cu003ec__DisplayClass64_0();
			V_0.specificulture = specificulture;
			V_0.postId = postId;
			V_0._context = _context;
			V_0._transaction = _transaction;
			stackVariable10 = V_0._context;
			if (stackVariable10 == null)
			{
				dummyVar0 = stackVariable10;
				stackVariable10 = new MixCmsContext();
			}
			V_1 = stackVariable10;
			stackVariable12 = V_0._transaction;
			if (stackVariable12 == null)
			{
				dummyVar1 = stackVariable12;
				stackVariable12 = V_1.get_Database().BeginTransaction();
			}
			V_2 = stackVariable12;
			try
			{
				try
				{
					stackVariable14 = V_1.get_MixModule();
					V_4 = Expression.Parameter(Type.GetTypeFromHandle(// 
					// Current member / type: Mix.Domain.Core.ViewModels.RepositoryResponse`1<System.Collections.Generic.List`1<Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel>> Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel::GetModulePostNavAsync(System.Int32,System.String,Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
					// Exception in: Mix.Domain.Core.ViewModels.RepositoryResponse<System.Collections.Generic.List<Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel>> GetModulePostNavAsync(System.Int32,System.String,Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
					// Specified method is not supported.
					// 
					// mailto: JustDecompilePublicFeedback@telerik.com


		public override MixModulePost ParseModel(MixCmsContext _context = null, IDbContextTransaction _transaction = null)
		{
			if (this.get_Id() == 0)
			{
				stackVariable7 = ViewModelBase<MixCmsContext, MixModulePost, Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel>.Repository;
				V_0 = Expression.Parameter(Type.GetTypeFromHandle(// 
				// Current member / type: Mix.Cms.Lib.Models.Cms.MixModulePost Mix.Cms.Lib.ViewModels.MixModulePosts.ReadViewModel::ParseModel(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
				// Exception in: Mix.Cms.Lib.Models.Cms.MixModulePost ParseModel(Mix.Cms.Lib.Models.Cms.MixCmsContext,Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
				// Specified method is not supported.
				// 
				// mailto: JustDecompilePublicFeedback@telerik.com

	}
}