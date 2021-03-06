﻿using AnkaCMS.Data.DataAccess.EntityFramework;
using AnkaCMS.Data.DataEntities;
using AnkaCMS.Service.Models;
using AnkaCMS.Core;
using AnkaCMS.Core.CrudBaseModels;
using AnkaCMS.Core.Exceptions;
using AnkaCMS.Core.Globalization;
using AnkaCMS.Core.Helpers;
using AnkaCMS.Core.Security;
using AnkaCMS.Core.Validation.FluentValidation;
using AnkaCMS.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using AnkaCMS.Service.Implementations.ValidationRules.FluentValidation;

namespace AnkaCMS.Service.Implementations
{
    public class MenuService : IMenuService
    {

        private readonly IRepository<Menu> _repositoryMenu;
        private readonly IRepository<MenuHistory> _repositoryMenuHistory;
        private readonly IRepository<User> _repositoryUser;
        private readonly IMainService _serviceMain;

        public MenuService(IRepository<Menu> repositoryMenu, IRepository<MenuHistory> repositoryMenuHistory, IRepository<User> repositoryUser, IMainService serviceMain)
        {
            _repositoryMenu = repositoryMenu;
            _repositoryMenuHistory = repositoryMenuHistory;
            _repositoryUser = repositoryUser;
            _serviceMain = serviceMain;
        }


        private User IdentityUser
        {
            get
            {
                // Thread'de kayıtlı kimlik bilgisi alınıyor
                var identity = (AnkaCMSIdentity)Thread.CurrentPrincipal.Identity;

                User user;

                // Veritabanından sorgulanıyor
                user = _repositoryUser.Get()
                    .Join(x => x.Person)
                    .FirstOrDefault(a => a.Id == identity.UserId);




                // Kullanıcı bulunamadı ise
                if (user == null)
                {
                    throw new NotFoundException(Messages.DangerIdentityUserNotFound);
                }

                return user;
            }
        }


        public ListModel<MenuModel> List(FilterModel filterModel)
        {
            var model = filterModel.CreateMapped<FilterModel, ListModel<MenuModel>>();
            return List(filterModel.StartDate, filterModel.EndDate, filterModel.PageNumber, filterModel.PageSize, filterModel.Status, filterModel.Searched, Guid.Empty, model);
        }

        public ListModel<MenuModel> List(FilterModelWithParent filterModel)
        {
            var model = filterModel.CreateMapped<FilterModelWithParent, ListModel<MenuModel>>();
            return List(filterModel.StartDate, filterModel.EndDate, filterModel.PageNumber, filterModel.PageSize, filterModel.Status, filterModel.Searched, filterModel.Parent.Id, model);
        }

        private ListModel<MenuModel> List(DateTime startDate, DateTime endDate, int pageNumber, int pageSize, int status, string searched, Guid parentId, ListModel<MenuModel> model)
        {
            var resetedStartDate = startDate.ResetTimeToStartOfDay();
            var resetedEndDate = endDate.ResetTimeToEndOfDay();

            Expression<Func<Menu, bool>> expression;

            if (model.Paging == null)
            {
                model.Paging = new Paging
                {
                    PageSize = pageSize,
                    PageNumber = pageNumber
                };
            }

            if (status != -1)
            {
                var bStatus = status.ToString().ToBoolean();
                if (searched != null)
                {
                    if (bStatus)
                    {
                        expression = c => c.IsApproved && c.Name.Contains(searched);
                    }
                    else
                    {
                        expression = c => c.IsApproved == false && c.Name.Contains(searched);
                    }
                }
                else
                {
                    if (bStatus)
                    {
                        expression = c => c.IsApproved;
                    }
                    else
                    {
                        expression = c => c.IsApproved == false;
                    }
                }

            }
            else
            {
                if (searched != null)
                {
                    expression = c => c.Name.Contains(searched);
                }
                else
                {
                    expression = c => c.Id != Guid.Empty;
                }
            }

            expression = expression.And(e => e.CreationTime >= resetedStartDate && e.CreationTime <= resetedEndDate);
            if (parentId != Guid.Empty)
            {
                expression = expression.And(e => e.ParentMenu.Id == parentId);
            }

            var sortHelper = new SortHelper<MenuModel>();
            var query = (IOrderedQueryable<Menu>)_repositoryMenu
                .Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person)
                .Join(x => x.ParentMenu)
                .Where(expression);
            sortHelper.OrderBy(x => x.DisplayOrder);

            model.Paging.TotalItemCount = query.Count();
            var items = model.Paging.PageSize > 0 ? query.Skip((model.Paging.PageNumber - 1) * model.Paging.PageSize).Take(model.Paging.PageSize) : query;
            var modelItems = new HashSet<MenuModel>();
            foreach (var item in items)
            {
                var modelItem = item.CreateMapped<Menu, MenuModel>();
                modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
                modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
                modelItem.ParentMenu = new IdCodeName(item.ParentMenu.Id, item.ParentMenu.Code, item.ParentMenu.Name);
                modelItems.Add(modelItem);
            }
            model.Items = modelItems.ToList();
            var pageSizeDescription = _serviceMain.ApplicationSettings.PageSizeList;
            var pageSizes = pageSizeDescription.Split(',').Select(s => new KeyValuePair<int, string>(s.ToInt(), s)).ToList();
            pageSizes.Insert(0, new KeyValuePair<int, string>(-1, "[" + Dictionary.All + "]"));
            model.Paging.PageSizes = pageSizes;
            model.Paging.PageCount = (int)Math.Ceiling((float)model.Paging.TotalItemCount / model.Paging.PageSize);
            if (model.Paging.TotalItemCount > model.Items.Count)
            {
                model.Paging.HasNextPage = true;
            }

            // ilk sayfa ise

            if (model.Paging.PageNumber == 1)
            {
                if (model.Paging.TotalItemCount > 0)
                {
                    model.Paging.IsFirstPage = true;
                }

                // tek sayfa ise

                if (model.Paging.PageCount == 1)
                {
                    model.Paging.IsLastPage = true;
                }

            }

            // son sayfa ise
            else if (model.Paging.PageNumber == model.Paging.PageCount)
            {
                model.Paging.HasNextPage = false;
                // tek sayfa değilse

                if (model.Paging.PageCount > 1)
                {
                    model.Paging.IsLastPage = true;
                    model.Paging.HasPreviousPage = true;
                }
            }

            // ara sayfa ise
            else
            {
                model.Paging.HasNextPage = true;
                model.Paging.HasPreviousPage = true;
            }

            if (model.Paging.TotalItemCount > model.Items.Count && model.Items.Count <= 0)
            {
                model.Message = Messages.DangerRecordNotFoundInPage;
            }

            if (model.Paging.TotalItemCount == 0)
            {
                model.Message = Messages.DangerRecordNotFound;
            }
            return model;
        }

        public DetailModel<MenuModel> Detail(Guid id)
        {
            var item = _repositoryMenu
                .Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person)
                .Join(x => x.ParentMenu)
                .FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }
            var modelItem = item.CreateMapped<Menu, MenuModel>();
            modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
            modelItem.ParentMenu = new IdCodeName(item.ParentMenu.Id, item.ParentMenu.Code, item.ParentMenu.Name);
            return new DetailModel<MenuModel>
            {
                Item = modelItem
            };
        }

        public AddModel<MenuModel> Add()
        {
            return new AddModel<MenuModel>
            {
                Item = new MenuModel
                {
                    IsApproved = false
                }
            };
        }

        public AddModel<MenuModel> Add(AddModel<MenuModel> addModel)
        {
            IValidator validator = new FluentValidator<MenuModel, MenuValidationRules>(addModel.Item);
            var validationResults = validator.Validate();
            if (!validator.IsValid)
            {
                throw new ValidationException(Messages.DangerInvalidEntitiy)
                {
                    ValidationResult = validationResults
                };
            }

            var parent = _repositoryMenu.Get(e => e.Id == addModel.Item.ParentMenu.Id);

            if (parent == null)
            {
                throw new NotFoundException(Messages.DangerParentNotFound);
            }

            var item = addModel.Item.CreateMapped<MenuModel, Menu>();

            if (_repositoryMenu.Get().FirstOrDefault(e => e.Code == item.Code) != null)
            {
                throw new DuplicateException(string.Format(Messages.DangerFieldDuplicated, Dictionary.Code));
            }

            item.Id = GuidHelper.NewGuid();
            item.Version = 1;
            item.CreationTime = DateTime.Now;
            item.LastModificationTime = DateTime.Now;
            item.ParentMenu = parent;
            item.DisplayOrder = 1;

            item.Creator = IdentityUser ?? throw new IdentityUserException(Messages.DangerIdentityUserNotFound);
            item.LastModifier = IdentityUser;
            _repositoryMenu.Add(item, true);
            var maxDisplayOrder = _repositoryMenu.Get().Max(e => e.DisplayOrder);
            item.DisplayOrder = maxDisplayOrder + 1;
            var affectedItem = _repositoryMenu.Update(item, true);
            var itemHistory = affectedItem.CreateMapped<Menu, MenuHistory>();
            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = affectedItem.Id;
            itemHistory.CreatorId = IdentityUser.Id;
            _repositoryMenuHistory.Add(itemHistory, true);

            addModel.Item = affectedItem.CreateMapped<Menu, MenuModel>();

            addModel.Item.Creator = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);
            addModel.Item.LastModifier = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);
            addModel.Item.ParentMenu = new IdCodeName(parent.Id, parent.Code, parent.Name);
            return addModel;
        }

        public UpdateModel<MenuModel> Update(Guid id)
        {
            var item = _repositoryMenu
                .Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person)
                .Join(x => x.ParentMenu)
               .FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }
            var modelItem = item.CreateMapped<Menu, MenuModel>();
            modelItem.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            modelItem.LastModifier = new IdCodeName(item.LastModifier.Id,item.LastModifier.Username, item.LastModifier.Person.DisplayName);
            modelItem.ParentMenu = new IdCodeName(item.ParentMenu.Id, item.ParentMenu.Code, item.ParentMenu.Name);

            return new UpdateModel<MenuModel>
            {
                Item = modelItem
            };
        }

        public UpdateModel<MenuModel> Update(UpdateModel<MenuModel> updateModel)
        {
            IValidator validator = new FluentValidator<MenuModel, MenuValidationRules>(updateModel.Item);
            var validationResults = validator.Validate();
            if (!validator.IsValid)
            {
                throw new ValidationException(Messages.DangerInvalidEntitiy)
                {
                    ValidationResult = validationResults
                };
            }

            var parent =
                _repositoryMenu.Get(x => x.Id == updateModel.Item.ParentMenu.Id && x.IsApproved);

            if (parent == null)
            {
                throw new NotFoundException(Messages.DangerParentNotFound);
            }

            var item = _repositoryMenu.Join(x => x.Creator.Person)
                .Join(x => x.LastModifier.Person)
                .Join(x => x.ParentMenu).FirstOrDefault(e => e.Id == updateModel.Item.Id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }



            if (updateModel.Item.Code != item.Code)
            {
                if (_repositoryMenu.Get().Any(p => p.Code == updateModel.Item.Code))
                {
                    throw new DuplicateException(string.Format(Messages.DangerFieldDuplicated, Dictionary.Code));
                }
            }

            var versionHistoryMenu = _repositoryMenuHistory.Get().Where(e => e.ReferenceId == item.Id).Max(t => t.Version);

            var itemHistory = item.CreateMapped<Menu, MenuHistory>();
            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = item.Id;
            itemHistory.CreatorId = IdentityUser.Id;
            itemHistory.IsDeleted = false;
            itemHistory.ParentMenuId = item.ParentMenu.Id;
            itemHistory.Version = versionHistoryMenu + 1;
            itemHistory.RestoreVersion = 0;
            _repositoryMenuHistory.Add(itemHistory, true);

            item.ParentMenu = parent;
            item.Code = updateModel.Item.Code;
            item.Name = updateModel.Item.Name;
            item.Description = updateModel.Item.Description;
            item.Address = updateModel.Item.Address;
            item.Icon = updateModel.Item.Icon;
            item.IsApproved = updateModel.Item.IsApproved;
            item.LastModificationTime = DateTime.Now;
            item.LastModifier = IdentityUser;

            item.LastModificationTime = DateTime.Now;
            item.LastModifier = IdentityUser;
            var version = item.Version;
            item.Version = version + 1;
            var affectedItem = _repositoryMenu.Update(item, true);

            updateModel.Item = affectedItem.CreateMapped<Menu, MenuModel>();


            updateModel.Item.Creator = new IdCodeName(item.Creator.Id, item.Creator.Username, item.Creator.Person.DisplayName);
            updateModel.Item.LastModifier = new IdCodeName(IdentityUser.Id,IdentityUser.Username, IdentityUser.Person.DisplayName);


            return updateModel;

        }

        public void Delete(Guid id)
        {
            var version = _repositoryMenuHistory.Get().Where(e => e.ReferenceId == id).Max(t => t.Version);
            var item = _repositoryMenu.Get(x => x.Id == id);
            if (item == null)
            {
                throw new NotFoundException(Messages.DangerRecordNotFound);
            }

            var itemHistory = item.CreateMapped<Menu, MenuHistory>();

            itemHistory.Id = GuidHelper.NewGuid();
            itemHistory.ReferenceId = item.Id;
            itemHistory.CreationTime = DateTime.Now;

            itemHistory.CreatorId = IdentityUser.Id;
            itemHistory.Version = version + 1;
            itemHistory.IsDeleted = true;

            _repositoryMenuHistory.Add(itemHistory, true);
            _repositoryMenu.Delete(item, true);
        }


        public List<IdCodeName> List()
        {
            var list = _repositoryMenu.Get().Where(x => x.IsApproved && x.ParentMenu.Code == "ADMINROOTMENU").OrderBy(x => x.DisplayOrder).Select(x => new IdCodeName(x.Id, x.Code, x.Name));
            if (list.Any())
            {
                return list.ToList();
            }
            throw new NotFoundException(Messages.DangerRecordNotFound);
        }

    }
}