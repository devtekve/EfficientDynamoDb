using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EfficientDynamoDb.DocumentModel;

namespace EfficientDynamoDb.Operations.BatchGetItem
{
    /// <summary>
    /// Represents a builder for the BatchGet operation.
    /// </summary>
    public interface IBatchGetEntityRequestBuilder
    {
        /// <summary>
        /// Executes the BatchGet operation and returns the list of entities.
        /// </summary>
        /// <param name="cancellationToken">Token that can be used to cancel the task.</param>
        /// <typeparam name="TEntity">Type of the DB entity.</typeparam>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<List<TEntity>> ToListAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class;

        /// <summary>
        /// Configures the operation to retrieve data from one or multiple tables in a batch.
        /// This method provides more granular control than <see cref="WithItems(EfficientDynamoDb.Operations.BatchGetItem.IBatchGetItemBuilder[])"/>, enabling read consistency configuration and attribute projections for each table separately.
        /// </summary>
        /// <param name="tables">An array of BatchGetTable builders, each of which configures a table in the batch retrieval operation.</param>
        /// <returns>BatchGet operation builder.</returns>
        IBatchGetEntityRequestBuilder FromTables(params IBatchGetTableBuilder[] tables);

        /// <summary>
        /// Configures the operation to retrieve data from one or multiple tables in a batch.
        /// This method provides more granular control than <see cref="WithItems(System.Collections.Generic.IEnumerable{EfficientDynamoDb.Operations.BatchGetItem.IBatchGetItemBuilder})"/>, enabling read consistency configuration and attribute projections for each table separately.
        /// </summary>
        /// <param name="tables">An enumerable collection of BatchGetTable builders, each of which configures a table in the batch retrieval operation.</param>
        /// <returns>BatchGet operation builder.</returns>
        IBatchGetEntityRequestBuilder FromTables(IEnumerable<IBatchGetTableBuilder> tables);

        /// <summary>
        /// Specify the items to retrieve in batch.
        /// </summary>
        /// <param name="items">BatchGet item builders.</param>
        /// <returns>BatchGet operation builder.</returns>
        /// <remarks>
        /// Use <see cref="Batch"/> static class to construct item builders.
        /// </remarks>
        IBatchGetEntityRequestBuilder WithItems(params IBatchGetItemBuilder[] items);

        /// <summary>
        /// Specify the items to retrieve in batch.
        /// </summary>
        /// <param name="items">BatchGet item builders.</param>
        /// <returns>BatchGet operation builder.</returns>
        /// <remarks>
        /// Use <see cref="Batch"/> static class to construct item builders.
        /// </remarks>
        IBatchGetEntityRequestBuilder WithItems(IEnumerable<IBatchGetItemBuilder> items);

        /// <summary>
        /// Represents the returned items as <see cref="Document"/>.
        /// </summary>
        /// <returns>BatchGet operation builder suitable for document response.</returns>
        IBatchGetDocumentRequestBuilder AsDocuments();
    }

    /// <summary>
    /// Represents a builder for the BatchGet operation.
    /// Provides methods for configuring options and executing the operation with a <see cref="Document"/> representation of the response.
    /// </summary>
    public interface IBatchGetDocumentRequestBuilder
    {
        /// <summary>
        /// Executes the BatchGet operation and returns the list of entities.
        /// Every entity is represented as <see cref="Document"/>.
        /// </summary>
        /// <param name="cancellationToken">Token that can be used to cancel the task.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<List<Document>> ToListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Configures the operation to retrieve data from one or multiple tables in a batch.
        /// This method provides more granular control than <see cref="WithItems(EfficientDynamoDb.Operations.BatchGetItem.IBatchGetItemBuilder[])"/>, enabling read consistency configuration and attribute projections for each table separately.
        /// </summary>
        /// <param name="tables">An array of BatchGetTable builders, each of which configures a table in the batch retrieval operation.</param>
        /// <returns>BatchGet operation builder.</returns>
        IBatchGetDocumentRequestBuilder FromTables(params IBatchGetTableBuilder[] tables);

        /// <summary>
        /// Configures the operation to retrieve data from one or multiple tables in a batch.
        /// This method provides more granular control than <see cref="WithItems(System.Collections.Generic.IEnumerable{EfficientDynamoDb.Operations.BatchGetItem.IBatchGetItemBuilder})"/>, enabling read consistency configuration and attribute projections for each table separately.
        /// </summary>
        /// <param name="tables">An enumerable collection of BatchGetTable builders, each of which configures a table in the batch retrieval operation.</param>
        /// <returns>BatchGet operation builder.</returns>
        IBatchGetDocumentRequestBuilder FromTables(IEnumerable<IBatchGetTableBuilder> tables);

        /// <summary>
        /// Specifies the items to retrieve in batch.
        /// </summary>
        /// <param name="items">BatchGetItem builders.</param>
        /// <returns>BatchGet operation builder.</returns>
        /// <remarks>
        /// Use <see cref="Batch"/> static class to construct item builders.
        /// </remarks>
        IBatchGetDocumentRequestBuilder WithItems(params IBatchGetItemBuilder[] items);

        /// <summary>
        /// Specifies the items to retrieve in batch.
        /// </summary>
        /// <param name="items">BatchGet item builders.</param>
        /// <returns>BatchGet operation builder.</returns>
        /// <remarks>
        /// Use <see cref="Batch"/> static class to construct item builders.
        /// </remarks>
        IBatchGetDocumentRequestBuilder WithItems(IEnumerable<IBatchGetItemBuilder> items);
    }
}