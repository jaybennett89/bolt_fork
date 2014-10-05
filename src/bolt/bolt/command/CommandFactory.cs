using System;
namespace Bolt {
  public interface ICommandFactory {
    Type TypeObject { get; }
    TypeId TypeId { get; }
    Command Create();
  }
}