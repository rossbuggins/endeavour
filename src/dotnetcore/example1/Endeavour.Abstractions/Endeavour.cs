using System;
namespace Endeavour
{

public class Endeavour<T> : IEndeavour
{
    public T Data {get;set;}
    public DateTimeOffset StartedDate { get; set; }
    public bool Completed { get; set; }
}
}