using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pazyn.EntityFrameworkCore.ExtendedProperties
{
    public class ExtendedPropertiesAnnotationProvider : IMigrationsAnnotationProvider
    {
        private IMigrationsAnnotationProvider MigrationsAnnotationProvider { get; }

        public ExtendedPropertiesAnnotationProvider(IMigrationsAnnotationProvider migrationsAnnotationProvider)
        {
            MigrationsAnnotationProvider = migrationsAnnotationProvider;
        }

        public IEnumerable<IAnnotation> ForRemove(IRelationalModel model)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(ITable table)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(IColumn column)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(IView view)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(IViewColumn column)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(IUniqueConstraint constraint)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(ITableIndex index)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(IForeignKeyConstraint foreignKey)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(ISequence sequence)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint)
        {
            throw new System.NotImplementedException();
        }
    }
}
