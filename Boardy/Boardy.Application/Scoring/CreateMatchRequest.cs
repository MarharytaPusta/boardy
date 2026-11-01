using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Scoring;

public record CreateMatchRequest
(
    Guid ScoreTemplateId,
    List<Guid> PlayerIds
);
