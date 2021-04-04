using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Linq;
using Microsoft.Extensions.Caching.Distributed;

using System.Reflection;
using FileManage.DBContexts;
using FileManage.DBModels;
using FileManage.Extensions.ExpressionEx;
using System.Linq.Expressions;

namespace FileManage.Controllers
{
    [ApiController]
    // [Authorize]
    public abstract partial class EntityController<TEntity, TController> : ControllerBase
    where TEntity : EntityBase
    where TController : ControllerBase
    {
        private readonly ILogger<TController> _logger;
        private readonly EntityContext _context;
        private readonly IDistributedCache _cache;

        public EntityController(ILogger<TController> logger, EntityContext context, IDistributedCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        [HttpGet("getAll")]
        public async virtual Task<IActionResult> getAllEntity()
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                // _context.Set<TEntity>().FromSqlRaw("");
                // await _cache.SetStringAsync(typeof(TController).Name + nameof(this.getAllEntity), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.getAllEntity)}");
                var result = await _context.Set<TEntity>().ToListAsync();

                apiResult.resultCode = 0;
                apiResult.resultBody = result;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }

        }

        [HttpGet("gettop/{top}")]
        public async virtual Task<IActionResult> getTopEntity(int top)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.getTopEntity)}");
                var result = await _context.Set<TEntity>().Take(top).ToListAsync();

                apiResult.resultCode = 0;
                apiResult.resultBody = result;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }

        }

        [HttpGet("get/{id}")]
        public async virtual Task<IActionResult> getEntityById(Guid id)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.getEntityById)}");
                var result = await _context.Set<TEntity>().FindAsync(id);

                apiResult.resultCode = 0;
                apiResult.resultBody = result;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }

        }

        [HttpPost("create")]
        public virtual async Task<IActionResult> createEntity(TEntity item)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.createEntity)}");
                var entityEntry = _context.Entry(item);
                entityEntry.State = EntityState.Added;
                var result = await _context.SaveChangesAsync();

                apiResult.resultCode = 0;
                apiResult.resultBody = entityEntry.Entity;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }
        [HttpPost("createMulti")]
        public virtual async Task<IActionResult> createMultiEntity(List<TEntity> items)
        {
            ApiResult apiResult = new ApiResult();
            List<TEntity> entityLsit = new List<TEntity>();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.createMultiEntity)}");
                foreach (var item in items)
                {
                    var entityEntry = _context.Entry(item);
                    entityEntry.State = EntityState.Added;
                    entityLsit.Add(entityEntry.Entity);
                }
                var result = await _context.SaveChangesAsync();
                apiResult.resultCode = 0;
                apiResult.resultBody = entityLsit;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }

        [HttpPost("update")]
        public virtual async Task<IActionResult> updateEntity(UpdateEntity<TEntity> updateEntity)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.updateEntity)}");
                var entry = _context.Entry(updateEntity.entity);
                var properties = updateEntity.properties;
                if (properties != null && properties.Length > 0)
                {
                    PropertyInfo[] entityProperty = typeof(TEntity).GetProperties();
                    properties = properties.Where(x => entityProperty.Select(s => s.Name).Contains(x)).ToArray();
                }
                {

                    if (properties == null || properties.Length == 0)
                    {
                        entry.State = EntityState.Modified;
                    }

                    properties.ToList().ForEach(x =>
                    {
                        entry.Property(x).IsModified = true;
                    });
                }
                var result = await _context.SaveChangesAsync();

                apiResult.resultCode = 0;
                apiResult.resultBody = entry.Entity;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }

        [HttpPost("updateMulti")]
        public virtual async Task<IActionResult> updateMultiEntity(List<UpdateEntity<TEntity>> updateEntitys)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.updateMultiEntity)}");
                var entityLsit = new List<TEntity>();
                if (updateEntitys != null && updateEntitys.Count > 0)
                {
                    foreach (var updateEntity in updateEntitys)
                    {
                        var entry = _context.Entry(updateEntity.entity);
                        var properties = updateEntity.properties;
                        if (properties != null && properties.Length > 0)
                        {
                            PropertyInfo[] entityProperty = typeof(TEntity).GetProperties();
                            properties = properties.Where(x => entityProperty.Select(s => s.Name).Contains(x)).ToArray();
                        }
                        {
                            if (properties == null || properties.Length == 0)
                            {
                                entry.State = EntityState.Modified;
                            }

                            properties.ToList().ForEach(x =>
                            {
                                entry.Property(x).IsModified = true;
                            });
                        }
                        entityLsit.Add(entry.Entity);
                    }
                    var result = await _context.SaveChangesAsync();
                }




                apiResult.resultCode = 0;
                apiResult.resultBody = entityLsit;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }

        [HttpPost("delete")]
        public virtual async Task<IActionResult> deleteEntity(TEntity item)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.deleteEntity)}");
                var entry = _context.Entry(item);
                entry.State = EntityState.Deleted;
                var result = await _context.SaveChangesAsync();
                apiResult.resultCode = 0;
                apiResult.resultBody = entry.Entity;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }

        [HttpPost("deleteMulti")]
        public virtual async Task<IActionResult> deleteMultiEntity(List<TEntity> items)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                var entityLsit = new List<TEntity>();
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.deleteMultiEntity)}");
                if (items != null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var entry = _context.Entry(item);
                        entry.State = EntityState.Deleted;
                        entityLsit.Add(entry.Entity);
                    }
                    var result = await _context.SaveChangesAsync();
                }
                apiResult.resultCode = 0;
                apiResult.resultBody = entityLsit;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }
        }


        [HttpPost("adsearch")]
        public async virtual Task<IActionResult> AdSearchEntity(AdvancedSearch advancedSearch)
        {
            ApiResult apiResult = new ApiResult();
            try
            {
                var conditions = advancedSearch?.entityConditions;
                var selectFileds = advancedSearch?.selectFileds;
                var queryDBSet = _context.Set<TEntity>();
                IQueryable<TEntity> queryWhere = null;
                IQueryable<dynamic> querySelect = null;
                
                if (conditions == null)
                {
                    queryWhere = queryDBSet.Where(x => 1 == 2);
                }
                else
                {
                    queryWhere = queryDBSet.QueryConditions(conditions);
                }
                if(selectFileds == null || selectFileds.Length == 0)
                {
                     querySelect=queryWhere.Select<TEntity, dynamic>(x=>x);
                }
                else
                {
                    querySelect=queryWhere.SelectByProperties<TEntity, dynamic>(selectFileds);
                    
                }

                await _cache.SetStringAsync(typeof(TController).Name + nameof(this.AdSearchEntity), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                _logger.LogInformation($"{typeof(TController).Name}/{nameof(this.AdSearchEntity)}");

                var result = await querySelect.ToListAsync();

                apiResult.resultCode = 0;
                apiResult.resultBody = result;
                return Ok(apiResult);
            }
            catch (Exception ex)
            {
                apiResult.resultCode = ResultCodeEnum.Exception;
                apiResult.resultBody = ex.ToString();
                return BadRequest(apiResult);
            }

        }

    }

}