import java.util.Collection;
import java.util.List;
import java.util.function.Predicate;
import java.util.stream.Collectors;

/**
 * @author andre
 * on 25/10/2017.
 */
public class Utilities {

    public static<T> List<T> filter(Predicate<T> criteria, Collection<T> list) {
        return list.stream().filter(criteria).collect(Collectors.<T>toList());
    }
}
