import axios, { AxiosInstance, AxiosResponse } from "axios";

import { buildURL } from "../tools";
import { NotifyService } from "./notifyService";
import { INotifyService } from "../abstractions";

export class ServiceFactory<TDetailsDTO, TDetails> {
    static http: AxiosInstance = axios;
    static getAsPost = false;

    private notifyService: NotifyService<TDetails>;
    EntityDetails: new (dto: TDetailsDTO) => TDetails;

    constructor(type: string, entity: new (dto: TDetailsDTO) => TDetails) {
        this.notifyService = new NotifyService<TDetails>(type);
        this.EntityDetails = entity;
    }

    create<T>(factory: (f: ServiceFactory<TDetailsDTO, TDetails>) => T): T {
        return factory(this);
    }

    createComplete<TInfos, TInfosDTO, TCreateDTO, TUpdateDTO, TFilterDTO>(
        manyURL: string | (() => string),
        oneURL: (id: string) => string,
        entityInfos: new (dto: TInfosDTO) => TInfos,
    ) {
        return this.create(factory => factory.build(
            factory.addNotify(),
            factory.addGetMany<TInfosDTO, TInfos, TFilterDTO>(manyURL, entityInfos),
            factory.addGet(id => oneURL(id)),
            factory.addCreate<TCreateDTO>(manyURL),
            factory.addUpdate<TUpdateDTO>(id => oneURL(id)),
            factory.addRemove(id => oneURL(id))
        ));
    }

    addGetMany<TInfosDTO, TInfos, TFilter>(url: string | (() => string), entity: new (dto: TInfosDTO) => TInfos)
        : { getMany: (filter?: TFilter) => Promise<TInfos[]> } {

        const getMany = async (filter?: TFilter, controller?: AbortController) => {
            const realUrl = typeof url === "string" ? url : url();

            let response;

            // If the service is configured to use GET as POST to prevent issues with large query strings,
            // we send the filter as a POST request with a "_method" parameter to indicate it's a GET request.
            if (ServiceFactory.getAsPost && filter) {
                response = await ServiceFactory.http.post(buildURL(realUrl, { "_method": "GET" }), filter, {
                    signal: controller?.signal
                });
            }
            else {
                response = await ServiceFactory.http.get(buildURL(realUrl, filter), {
                    signal: controller?.signal
                });
            }

            const dtos: TInfosDTO[] = response.data;

            return dtos.map(dto => new entity(dto));
        }

        return { getMany };
    }


    addGet(url: (id: string) => string)
        : { get: (id: string) => Promise<TDetails> } {

        const get = async (id: string) => {
            const response = await ServiceFactory.http.get(url(id));
            const dto: TDetailsDTO = response.data;

            const result = new this.EntityDetails(dto);

            return result;
        }

        return { get };
    }

    static addCustom<T extends string, TArgs extends any[], TResultDTO, TResult>(name: T, call: (axios: AxiosInstance, ...args: TArgs) => Promise<AxiosResponse>, mapper: (dto: TResultDTO) => TResult): Record<T, (...args: TArgs) => Promise<TResult>> {

        const fetch = async (...args: TArgs) => {
            const response = await call(ServiceFactory.http, ...args);
            const dto: TResultDTO = response.data;

            const result = mapper(dto);

            return result;
        }

        return { [name]: fetch } as Record<T, (...args: TArgs) => Promise<TResult>>;
    }

    addCreate<TCreateDTO>(url: string | (() => string))
        : { create: (dto: TCreateDTO) => Promise<TDetails> } {

        const create = async (dto: TCreateDTO) => {
            const realUrl = typeof url === "string" ? url : url();
            const response = await ServiceFactory.http.post(realUrl, dto);
            const result = new this.EntityDetails(response.data);

            if (this.notifyService)
                this.notifyService.notify("add", result);

            return result;
        }

        return { create };
    }

    addUpdate<TUpdateDTO>(url: (id: string) => string)
        : { update: (id: string, dto: TUpdateDTO) => Promise<TDetails> } {

        const update = async (id: string, dto: TUpdateDTO) => {
            const response = await ServiceFactory.http.post(url(id), dto);
            const result = new this.EntityDetails(response.data);

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

    addNotify<U = {}>(others?: (notifyService: NotifyService<TDetails>) => U): INotifyService<TDetails> & U {
        if (!this.notifyService) throw new Error("Create your service with a type if you want to use notify");

        const notifyService = this.notifyService

        const { subscribe, unsubscribe } = notifyService;

        const otherMethods: U = others ? others(notifyService) : {} as U;

        return { subscribe: subscribe.bind(notifyService), unsubscribe: unsubscribe.bind(notifyService), ...otherMethods };
    }


    build<T>(target: T): T;
    build<T, U>(target: T, source1: U): T & U;
    build<T, U, V>(target: T, source1: U, source2: V): T & U & V;
    build<T, U, V, W>(target: T, source1: U, source2: V, source3: W): T & U & V & W
    build<T, U, V, W, X>(target: T, source1: U, source2: V, source3: W, source4: X): T & U & V & W & X
    build<T, U, V, W, X, Y>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y): T & U & V & W & X & Y
    build<T, U, V, W, X, Y, Z>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z): T & U & V & W & X & Y & Z
    build<T, U, V, W, X, Y, Z, Z1>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1): T & U & V & W & X & Y & Z & Z1
    build<T, U, V, W, X, Y, Z, Z1, Z2>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2): T & U & V & W & X & Y & Z & Z1 & Z2
    build<T, U, V, W, X, Y, Z, Z1, Z2, Z3>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2, source9: Z3): T & U & V & W & X & Y & Z & Z1 & Z2 & Z3
    build<T, U, V, W, X, Y, Z, Z1, Z2, Z3, Z4>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2, source9: Z3, source10: Z4): T & U & V & W & X & Y & Z & Z1 & Z2 & Z3 & Z4
    build<T, U, V, W, X, Y, Z, Z1, Z2, Z3, Z4, Z5>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2, source9: Z3, source10: Z4, source11: Z5): T & U & V & W & X & Y & Z & Z1 & Z2 & Z3 & Z4 & Z5
    build<T, U, V, W, X, Y, Z, Z1, Z2, Z3, Z4, Z5, Z6>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2, source9: Z3, source10: Z4, source11: Z5, source12: Z6): T & U & V & W & X & Y & Z & Z1 & Z2 & Z3 & Z4 & Z5 & Z6
    build<T, U, V, W, X, Y, Z, Z1, Z2, Z3, Z4, Z5, Z6, Z7>(target: T, source1: U, source2: V, source3: W, source4: X, source5: Y, source6: Z, source7: Z1, source8: Z2, source9: Z3, source10: Z4, source11: Z5, source12: Z6, source13: Z7): T & U & V & W & X & Y & Z & Z1 & Z2 & Z3 & Z4 & Z5 & Z6 & Z7
    build() {
        return Object.assign({}, ...arguments)
    }
}
