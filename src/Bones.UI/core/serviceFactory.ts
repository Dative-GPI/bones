import axios, { AxiosInstance } from "axios";

import { buildURL } from "../tools";
import { NotifyService } from "./notifyService";
import { AllCallback, INotifyService } from "../abstractions";

export class ServiceFactory {
    static http: AxiosInstance = axios;

    private notifyService: NotifyService<unknown> | null;

    private constructor(type?: string) {
        if (type) this.notifyService = new NotifyService(type);
    }

    static create<T>(type: string, factory: (f: ServiceFactory) => T): () => T {
        const f = new ServiceFactory(type);

        return () => factory(f);
    }

    static createComplete<TInfos, TInfosDTO, TDetails, TDetailsDTO, TCreateDTO, TUpdateDTO, TFilterDTO>(
        entityName: string,
        manyURL: string | (() => string),
        oneURL: (id: string) => string,
        entityDetails: new (dto: TDetailsDTO) => TDetails,
        entityInfos: new (dto: TInfosDTO) => TInfos,
    ) {
        return ServiceFactory.create(entityName, factory => factory.build(
            factory.addNotify<TDetails>(),
            factory.addGetMany<TInfosDTO, TInfos, TFilterDTO>(manyURL, entityInfos),
            factory.addGet<TDetailsDTO, TDetails>(id => oneURL(id), entityDetails),
            factory.addCreate<TCreateDTO, TDetailsDTO, TDetails>(manyURL, entityDetails),
            factory.addUpdate<TUpdateDTO, TDetailsDTO, TDetails>(id => oneURL(id), entityDetails),
            factory.addRemove(id => oneURL(id))
        ));
    }

    addGetMany<TInfosDTO, TInfos, TFilter>(url: string | (() => string), entity: new (dto: TInfosDTO) => TInfos)
        : { getMany: (filter?: TFilter) => Promise<TInfos[]> } {

        const getMany = async (filter?: TFilter) => {
            const realUrl = typeof url === "string" ? url : url();
            const response = await ServiceFactory.http.get(buildURL(realUrl, filter));
            const dtos: TInfosDTO[] = response.data;

            return dtos.map(dto => new entity(dto));
        }

        return { getMany };
    }


    addGet<TDetailsDTO, TDetails>(url: (id: string) => string, entity: new (dto: TDetailsDTO) => TDetails)
        : { get: (id: string) => Promise<TDetails> } {

        const get = async (id: string) => {
            const response = await ServiceFactory.http.get(url(id));
            const dto: TDetailsDTO = response.data;

            const result = new entity(dto);

            return result;
        }

        return { get };
    }

    addCreate<TCreateDTO, TDetailsDTO, TDetails>(url: string | (() => string), entity: new (dto: TDetailsDTO) => TDetails)
        : { create: (dto: TCreateDTO) => Promise<TDetails> } {

        const create = async (dto: TCreateDTO) => {
            const realUrl = typeof url === "string" ? url : url();
            const response = await ServiceFactory.http.post(realUrl, dto);
            const result = new entity(response.data);

            if (this.notifyService)
                this.notifyService.notify("add", result);

            return result;
        }

        return { create };
    }

    addUpdate<TUpdateDTO, TDetailsDTO, TDetails>(url: (id: string) => string, entity: new (dto: TDetailsDTO) => TDetails)
        : { update: (id: string, dto: TUpdateDTO) => Promise<TDetails> } {

        const update = async (id: string, dto: TUpdateDTO) => {
            const response = await ServiceFactory.http.post(url(id), dto);
            const result = new entity(response.data);

            if (this.notifyService)
                this.notifyService.notify("update", result);

            return result;
        }

        return { update };
    }

    addRemove(url: (id: string) => string)
        : { remove: (id: string) => Promise<void> } {

        const remove = async (id: string) => {
            await ServiceFactory.http.delete(url(id));

            if (this.notifyService)
                this.notifyService.notify("delete", id);
        }

        return { remove };
    }

    addNotify<TEntity, U = {}>(others?: (notifyService: INotifyService<TEntity>) => U): { subscribe: (event: "add" | "update" | "delete" | "all", callback: AllCallback<TEntity>) => number, unsubscribe: (id: number) => void } & U {
        if (!this.notifyService) throw new Error("Create your service with a type if you want to use notify");

        const notifyService = (this.notifyService as NotifyService<TEntity>)

        const { subscribe, unsubscribe } = notifyService;

        const otherMethods: U = others ? others(notifyService) : {} as U;

        return { subscribe, unsubscribe, ...otherMethods };
    }


    build<T, U>(target: T, source1: U): T & U;
    build<T, U, V>(target: T, source1: U, source2: V): T & U & V;
    build<T, U, V, W>(target: T, source1: U, source2: V, source3: W): T & U & V & W
    build<T, U, V, W, X>(target: T, source1: U, source2: V, source3: W, source4: X): T & U & V & W & X
    build<T, U, V, W, X, Y>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y): T & U & V & W & X & Y
    build<T, U, V, W, X, Y, Z>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z): T & U & V & W & X & Y & Z
    build<T>(target: T): T {
        return {
            ...target,
            ...arguments
        }
    }
}
