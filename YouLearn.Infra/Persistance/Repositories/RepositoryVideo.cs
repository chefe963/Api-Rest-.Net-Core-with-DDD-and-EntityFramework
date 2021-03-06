﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YouLearn.Domain.Entities;
using YouLearn.Domain.Interfaces.Repositories;
using YouLearn.Infra.Persistance.EF;

namespace YouLearn.Infra.Persistance.Repositories
{
    public class RepositoryVideo : IRepositoryVideo
    {
        private readonly YouLearnContext _context;

        public RepositoryVideo(YouLearnContext context)
        {
            _context = context;
        }

        public void Add(Video video)
        {
            _context.Videos.Add(video);
        }

        public bool ExisteCanalAssociado(Guid idCanal)
        {
            return _context.Videos.Any(x => x.Canal.Id == idCanal);
        }

        public bool ExistePlayListAssociada(Guid idPlayList)
        {
            return _context.Videos.Any(x => x.PlayList.Id == idPlayList);
        }

        public IEnumerable<Video> ListaVideos()
        {
            // asQueryable montou somente a query
            var query = _context.Videos.Include(x => x.Canal).Include(x => x.PlayList).AsQueryable();

            return query.ToList();
        }

        public IEnumerable<Video> ListaVideos(string tags)
        {
            // asQueryable montou somente a query
            var query = _context.Videos.Include(x => x.Canal).Include(x => x.PlayList).AsQueryable();

            tags.Split(' ').ToList().ForEach(tag => {
                query = query.Where(x => x.Tags.Contains(tag) || x.Titulo.Contains(tag) || x.Descricao.Contains(tag));
            });

            return query.ToList();
        }

        public IEnumerable<Video> ListaVideos(Guid idPlayList)
        {
            var query = _context.Videos.Include(x => x.Canal).Include(x => x.PlayList)
                .Where(x => x.PlayList.Id ==idPlayList).ToList();

            return query;
        }


    }
}
