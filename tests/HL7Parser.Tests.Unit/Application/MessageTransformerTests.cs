// Copyright (c) Christopher Wenzlick. All rights reserved.

using System.Collections.Generic;
using HL7Parser.Application.Transformation;
using HL7Parser.Application.UseCases;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application;

public class MessageTransformerTests
{
    private const string RawMessage =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||PATIENT123^^^MRN||ORIGINAL_PID5\r" +
        "OBX|1|ST|TEST^Result||ORIGINAL_OBX5";

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRulesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageTransformer(null!));
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        var transformer = new MessageTransformer([]);
        Assert.Throws<ArgumentNullException>(() => transformer.Execute(null!));
    }

    [Fact]
    public void Execute_ReturnsInputMessage_WhenRulesListIsEmpty()
    {
        var transformer = new MessageTransformer([]);
        var message = CreateMessage(RawMessage);

        Message result = transformer.Execute(message);

        Assert.Same(message, result);
    }

    [Fact]
    public void Execute_ThreadsOutputOfEachRuleIntoNext_WhenMultipleRulesAreApplied()
    {
        var message1 = CreateMessage(RawMessage);
        var message2 = CreateMessage(RawMessage);
        var message3 = CreateMessage(RawMessage);

        var rule1 = new RecordingRule(output: message2);
        var rule2 = new RecordingRule(output: message3);
        var transformer = new MessageTransformer([rule1, rule2]);

        Message result = transformer.Execute(message1);

        Assert.Same(message1, rule1.ReceivedInput);
        Assert.Same(message2, rule2.ReceivedInput);
        Assert.Same(message3, result);
    }

    [Fact]
    public void Execute_AppliesRulesInOrder_ProducingCumulativeEffect()
    {
        var rule1 = new FieldCopyRule("PID", 3, "OBX", 5);
        var rule2 = new FieldCopyRule("OBX", 5, "PID", 5);
        var transformer = new MessageTransformer([rule1, rule2]);
        var message = CreateMessage(RawMessage);

        Message result = transformer.Execute(message);

        Result<Field> pid5 = result.GetSegments("PID")[0].GetField(5);
        Assert.True(pid5.IsSuccess);
        Assert.Equal("PATIENT123", pid5.Value[0][0][0].RawValue);
    }

    private static Message CreateMessage(string raw)
    {
        Result<Message> result = Message.Create(raw);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class RecordingRule : ITransformRule
    {
        private readonly Message _output;

        public Message? ReceivedInput { get; private set; }

        public RecordingRule(Message output) => _output = output;

        public Message Apply(Message message)
        {
            ReceivedInput = message;
            return _output;
        }
    }
}
