using System;
namespace Endeavour
{

public interface IEndeavour
{
    DateTimeOffset StartedDate { get; set; }
    bool Completed { get; set; }
}
}