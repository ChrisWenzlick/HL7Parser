// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.UseCases;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application;

public class MessageParserTests
{
    private const string ValidMsh = "MSH|^~\\&|sending|receiving|||||ADT^A01||P|2.5";

    private readonly IMessageParser _messageParser = new MessageParser();

    [Fact]
    public void Execute_ReturnsSuccess_WhenMessageIsValid()
    {
        Result<Message> result = _messageParser.Execute(ValidMsh);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Execute_Throws_WhenRawMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _messageParser.Execute(null!));
    }

    [Fact]
    public void Execute_ReturnsFailure_WhenRawMessageIsEmpty()
    {
        Result<Message> result = _messageParser.Execute(string.Empty);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Execute_ReturnsFailure_WhenMessageIsMalformed()
    {
        Result<Message> result = _messageParser.Execute("PID|1||123456^^^MRN||DOE^JOHN");
        Assert.False(result.IsSuccess);
    }
}
