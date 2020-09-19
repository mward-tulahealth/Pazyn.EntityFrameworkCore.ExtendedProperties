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

        public IEnumerable<IAnnotation> For(IModel model) =>
            MigrationsAnnotationProvider.For(model);

        public IEnumerable<IAnnotation> For(IIndex index) =>
            MigrationsAnnotationProvider.For(index);

        public IEnumerable<IAnnotation> For(IProperty property) =>
            MigrationsAnnotationProvider.For(property)
                .Concat(property.GetAnnotations().Where(a => a.Name == nameof(ExtendedProperty)));

        public IEnumerable<IAnnotation> For(IKey key) =>
            MigrationsAnnotationProvider.For(key);

        public IEnumerable<IAnnotation> For(IForeignKey foreignKey) =>
            MigrationsAnnotationProvider.For(foreignKey);

        public IEnumerable<IAnnotation> For(IEntityType entityType) =>
            MigrationsAnnotationProvider.For(entityType)
                .Concat(entityType.GetAnnotations().Where(a => a.Name == nameof(ExtendedProperty)));

        public IEnumerable<IAnnotation> For(ISequence sequence) =>
            MigrationsAnnotationProvider.For(sequence);

        public IEnumerable<IAnnotation> For(ICheckConstraint checkConstraint) =>
            MigrationsAnnotationProvider.For(checkConstraint);

        public IEnumerable<IAnnotation> ForRemove(IModel model) =>
            MigrationsAnnotationProvider.For(model);

        public IEnumerable<IAnnotation> ForRemove(IIndex index) =>
            MigrationsAnnotationProvider.For(index);

        public IEnumerable<IAnnotation> ForRemove(IProperty property) =>
            MigrationsAnnotationProvider.For(property);

        public IEnumerable<IAnnotation> ForRemove(IKey key) =>
            MigrationsAnnotationProvider.For(key);

        public IEnumerable<IAnnotation> ForRemove(IForeignKey foreignKey) =>
            MigrationsAnnotationProvider.For(foreignKey);

        public IEnumerable<IAnnotation> ForRemove(IEntityType entityType) =>
            MigrationsAnnotationProvider.For(entityType);

        public IEnumerable<IAnnotation> ForRemove(ISequence sequence) =>
            MigrationsAnnotationProvider.For(sequence);

        public IEnumerable<IAnnotation> ForRemove(ICheckConstraint checkConstraint) =>
            MigrationsAnnotationProvider.For(checkConstraint);
    }
}