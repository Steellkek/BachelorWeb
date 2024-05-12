namespace BachelorWeb.Intarfaces;

public interface IRepository<T>
    where T : class
{
    IEnumerable<T> GetList(); 
    T Get(long id); 
    void Create(T item); 
    void Update(T item); 
    void Delete(long id); 
    void Save();  
}