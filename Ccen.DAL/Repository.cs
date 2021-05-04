using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core;

namespace Amazon.DAL
{
    public class Repository<T> : IRepository<T> where T : class
    {
        #region Members

        //private readonly ILog logger = LoggerFactory.GetLogger("Repository");
        protected readonly IQueryableUnitOfWork unitOfWork;

        #endregion

        #region Constructor

        public Repository(IQueryableUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException("unitOfWork");
            }

            this.unitOfWork = unitOfWork;
        }

        #endregion

        #region IRepository<T> Members

        public IUnitOfWork UnitOfWork { get { return unitOfWork; } }

        /// <summary>
        /// Add item w/o commit
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(T item)
        {
            if (item != null)
            {
                GetSet().Add(item); // add new item in this set
            }
        }

        /// <summary>
        /// Remove item w/o commit
        /// </summary>
        /// <param name="item"></param>
        public virtual void Remove(T item)
        {
            if (item != null)
            {
                unitOfWork.Attach(item);
                GetSet().Remove(item);
            }
        }

        public virtual void Modify(T item)
        {
            if (item != null)
            {
                unitOfWork.SetModified(item);
            }
        }

        public virtual void TrackItem(T item)
        {
            if (item != null)
            {
                unitOfWork.Attach(item);
            }
        }

        public virtual void TrackItem(T item, IList<Expression<Func<T, object>>> modifiedFieldList)
        {
            if (item != null)
            {
                unitOfWork.Attach(item);

                if (modifiedFieldList != null)
                {
                    foreach (var field in modifiedFieldList)
                        unitOfWork.Context.Entry(item).Property(field).IsModified = true;
                }
            }
        }

        public virtual T GetLocalOrAttach(Func<T, bool> searchLocalQuery, Func<T> getAttachItem)
        {
            T localEntity = GetSet().Local.FirstOrDefault(searchLocalQuery);

            if (localEntity == null)
            {
                localEntity = getAttachItem();
                TrackItem(localEntity);
            }

            return localEntity;
        }

        public virtual void Merge(T persisted, T current)
        {
            unitOfWork.ApplyCurrentValues(persisted, current);
        }

        public virtual T Get(int id)
        {
            return id != 0 ? GetSet().Find(id) : null;
        }

        public virtual T Get(long id)
        {
            return id != 0 ? GetSet().Find(id) : null;
        }

        public virtual T Get(string id)
        {
            return !String.IsNullOrEmpty(id) ? GetSet().Find(id) : null;
        }

        public virtual IQueryable<T> GetAll()
        {
            return GetSet();
        }

        public virtual IQueryable<T> GetPaged<TProperty>(int pageIndex, int pageCount,
            Expression<Func<T, TProperty>> orderByExpression, bool @ascending)
        {
            var set = GetSet();

            if (ascending)
            {
                return set.OrderBy(orderByExpression)
                    .Skip(pageCount * pageIndex)
                    .Take(pageCount);
            }
            return set.OrderByDescending(orderByExpression)
                .Skip(pageCount * pageIndex)
                .Take(pageCount);
        }

        public virtual IQueryable<T> GetFiltered(Expression<Func<T, bool>> filter)
        {
            return GetSet().Where(filter);
        }

        public void Dispose()
        {
            if (unitOfWork != null)
            {
                unitOfWork.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private IDbSet<T> GetSet()
        {
            return unitOfWork.GetSet<T>();
        }

        #endregion
    }
}
