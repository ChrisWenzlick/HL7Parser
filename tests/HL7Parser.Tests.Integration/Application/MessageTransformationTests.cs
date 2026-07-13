// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Transformation;
using HL7Parser.Application.UseCases;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Integration.Application;

public class MessageTransformationTests
{
    private readonly IMessageParser _messageParser = new MessageParser();

    [Fact]
    public void ParseThenTransform_CopiesFieldAndReflectsChangeInToHl7String_WhenFieldCopyRuleIsApplied()
    {
        const string rawMessage =
            "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
            "PID|1||PATIENT123^^^MRN||DOE^JOHN\r" +
            "OBX|1|ST|TEST^Result||ORIGINAL";

        Result<Message> parseResult = _messageParser.Execute(rawMessage);
        Assert.True(parseResult.IsSuccess);

        var transformer = new MessageTransformer([new FieldCopyRule("PID", 5, "OBX", 5)]);
        Message transformed = transformer.Execute(parseResult.Value);

        Result<Field> obx5 = transformed.GetSegments("OBX")[0].GetField(5);
        Assert.True(obx5.IsSuccess);
        Assert.Equal("DOE", obx5.Value[0][0][0].RawValue);

        string hl7String = transformed.ToHl7String();
        Assert.Contains("DOE^JOHN", hl7String);
    }
}
