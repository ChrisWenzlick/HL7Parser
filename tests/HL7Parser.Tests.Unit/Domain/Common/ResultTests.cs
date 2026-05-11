// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain.Common
{
    public class ResultTests
    {
        [Fact]
        public void Success_IsSuccess_ReturnsTrue()
        {
            var result = Result<string>.Success("test value");

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Failure_IsSuccess_ReturnsTrue()
        {
            var result = Result<string>.Failure("something went wrong");

            Assert.False(result.IsSuccess);
        }
    }
}
