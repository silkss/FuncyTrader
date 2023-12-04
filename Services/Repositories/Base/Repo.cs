using System.Collections.Generic;

namespace Services.Repositories.Base;
public class Repo<Type> {
    private readonly List<Type> items = [];
    public IEnumerable<Type> GetAll() => items;
    public bool Add(Type item) {
        items.Add(item);
        return true;
    }
}
