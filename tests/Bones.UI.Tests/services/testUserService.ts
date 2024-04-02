import { ServiceFactory, ComposableFactory } from '@dative-gpi/bones-ui';
import { CreateTestUserDTO, TestUserDetails, TestUserDetailsDTO, UpdateTestUserDTO } from '../models/testUserDetails';
import { TestUserFilter, TestUserInfos, TestUserInfosDTO } from '../models/testUserInfos';

export const TEST_USERS_URL = "/api/testUsers";
export const TEST_USER_URL = (id: string) => `/api/testUsers/${id}`;

const testUserServiceFactory = new ServiceFactory<TestUserDetailsDTO, TestUserDetails>("test", TestUserDetails)
    .createComplete<TestUserInfos, TestUserInfosDTO, CreateTestUserDTO, UpdateTestUserDTO, TestUserFilter>(TEST_USERS_URL, TEST_USER_URL, TestUserInfos);

const AccountLoginFactory = new ServiceFactory<Boolean, Boolean>("account-login", Boolean)
    .create(f => f.build(
        f.addNotify(),
        ServiceFactory.addCustom("login", (axios, d: CreateTestUserDTO) => axios.post(TEST_USERS_URL, d), (dto: TestUserDetailsDTO) => new Array(5).map(a => new TestUserDetails(dto))),
        ServiceFactory.addCustom("logout", axios => axios.get(TEST_USERS_URL), (dto: TestUserDetailsDTO) => new TestUserDetails(dto)),
    ));

export const useTestUsersSync = ComposableFactory.sync<TestUserDetails, TestUserInfos>(testUserServiceFactory);
export const useTestUserTrack = ComposableFactory.track(testUserServiceFactory);


export const useLogin = ComposableFactory.custom(AccountLoginFactory.login, () => {
    const { sync } = useTestUsersSync();
    return (entities) => {
        sync(entities.value);
    }
});

export const useLogout = ComposableFactory.custom(AccountLoginFactory.logout, () => {
    const { track } = useTestUserTrack();
    return (entity) => {
        track(entity);
    }
});

export const useTestUser = ComposableFactory.get(testUserServiceFactory, () => {
    const { synceds, sync } = useTestUsersSync();
    return (entity) => {
        // sync(entity.childs);
    }
});

export const useTestUsers = ComposableFactory.getMany(testUserServiceFactory);
export const useCreateTestUser = ComposableFactory.create(testUserServiceFactory);
export const useUpdateTestUser = ComposableFactory.update(testUserServiceFactory);
export const useRemoveTestUser = ComposableFactory.remove(testUserServiceFactory);


const testGet = new ServiceFactory<TestUserDetailsDTO, TestUserDetails>("test", TestUserDetails)
    .create(f => f.build(
        f.addGetMany<TestUserInfosDTO, TestUserInfos, TestUserFilter>(TEST_USERS_URL, TestUserInfos),
        f.addNotify()
    ));

const { entity, get, getting } = useTestUser();
const { entities, fetching, getMany } = useTestUsers();
const { created, create, creating } = useCreateTestUser();
const { updated, update, updating } = useUpdateTestUser();
const { remove, removing } = useRemoveTestUser();
const { synceds, sync } = useTestUsersSync();
