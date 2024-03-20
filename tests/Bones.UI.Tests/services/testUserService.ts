import { ServiceFactory, ComposableFactory } from '@dative-gpi/bones-ui';
import { CreateTestUserDTO, TestUserDetails, TestUserDetailsDTO, UpdateTestUserDTO } from '../models/testUserDetails';
import { TestUserFilter, TestUserInfos, TestUserInfosDTO } from '../models/testUserInfos';

export const TEST_USERS_URL = "/api/testUsers";
export const TEST_USER_URL = (id: string) => `/api/testUsers/${id}`;

const testUserServiceFactory = new ServiceFactory<TestUserDetailsDTO, TestUserDetails>("test", TestUserDetails)
    .createComplete<TestUserInfos, TestUserInfosDTO, CreateTestUserDTO, UpdateTestUserDTO, TestUserFilter>(TEST_USERS_URL, TEST_USER_URL, TestUserInfos);

export const useTestUser = ComposableFactory.get(testUserServiceFactory);
export const useTestUsers = ComposableFactory.getMany(testUserServiceFactory);
export const useCreateTestUser = ComposableFactory.create(testUserServiceFactory);
export const useUpdateTestUser = ComposableFactory.update(testUserServiceFactory);
export const useRemoveTestUser = ComposableFactory.remove(testUserServiceFactory);

export const useTestUsersSync = ComposableFactory.sync<TestUserDetails, TestUserInfos>(testUserServiceFactory);

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
const { synceds } = useTestUsersSync([]);
