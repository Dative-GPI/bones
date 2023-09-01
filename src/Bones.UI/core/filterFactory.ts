export class FilterFactory {
    static create<TFilter>(filter: TFilter): (el: any) => boolean {
        const filterMethod = (el: any) => {
            for (const key in filter) {
                if (el[key] !== filter[key]) {
                    return false;
                }
            }
            return true;
        }

        return filterMethod;
    }
}