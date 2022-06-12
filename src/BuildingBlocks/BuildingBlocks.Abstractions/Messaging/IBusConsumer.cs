namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusConsumer
{
    /// <summary>
    /// consume the message, with specified subscribeMethod.
    /// </summary>
    /// <param name="handler">handler to execute the message.</param>
    /// <param name="consumeBuilder"></param>
    /// <typeparam name="TMessage">A type that implements the <see cref="IMessage"/>.</typeparam>
    void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage;

    /// <summary>
    /// consume the message, with specified subscribeMethod.
    /// </summary>
    /// <param name="subscribeMethod">The delegate handler to execute the message.</param>
    /// <param name="consumeBuilder"></param>
    /// <typeparam name="TMessage">A type that implements the <see cref="IMessage"/>.</typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Consume<TMessage>(
        MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    /// <summary>
    /// Consume a message with <see cref="TMessage"/> message type and discovering a message handler that implements <see cref="IMessageHandler{TMessage}"/> interface for this type.
    /// </summary>
    /// <typeparam name="TMessage">A type that implements the <see cref="IMessage"/>.</typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    /// <summary>
    /// Consume a message with specific message type with discovering a message handler that implements <see cref="IMessageHandler{TMessage}"/> interface for this type.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Consume(Type messageType, CancellationToken cancellationToken = default);


    /// <summary>
    /// Consume a message with <see cref="TMessage"/> type and <see cref="THandler"/> handler.
    /// </summary>
    /// <typeparam name="THandler">A type that implements the <see cref="IMessageHandler{TMessage}"/> interface.</typeparam>
    /// <typeparam name="TMessage">A type that implements the <see cref="IMessage"/>.</typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage;

    /// <summary>
    /// consume all messages that implements the <see cref="IMessageHandler{TMessage}"/> interface.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConsumeAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// consume all messages that implements the <see cref="IMessageHandler{TMessage}"/> interface from the assembly of the provided type
    /// </summary>
    /// <typeparam name="TType">A type for discovering associated assembly.</typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConsumeAllFromAssemblyOf<TType>(CancellationToken cancellationToken = default);

    /// <summary>
    /// consume all messages that implements the <see cref="IMessageHandler{TMessage}"/> interface from the assemblies of the provided types
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="assemblyMarkerTypes">Types for discovering associated assemblies.</param>
    /// <returns></returns>
    Task ConsumeAllFromAssemblyOf(CancellationToken cancellationToken = default, params Type[] assemblyMarkerTypes);
}
