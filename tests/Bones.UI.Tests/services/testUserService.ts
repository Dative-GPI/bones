import { ServiceFactory, ComposableFactory } from '@dative-gpi/bones-ui';
import { CreateTestUserDTO, TestUserDetails, UpdateTestUserDTO } from '../models/testUserDetails';
import { TestUserFilter, TestUserInfos } from '../models/testUserInfos';

export const TEST_USERS_URL = "/api/testUsers";
export const TEST_USER_URL = (id: string) => `/api/testUsers/${id}`;

const testUserServiceFactory = ServiceFactory.createComplete("testUser", TEST_USERS_URL, TEST_USER_URL, TestUserDetails, TestUserInfos);

export const useTestUser = ComposableFactory.get<TestUserDetails>(testUserServiceFactory);
export const useTestUsers = ComposableFactory.getMany<TestUserInfos, TestUserFilter>(testUserServiceFactory);
export const useCreateTestUser = ComposableFactory.create<CreateTestUserDTO, TestUserDetails>(testUserServiceFactory);
export const useUpdateTestUser = ComposableFactory.update<UpdateTestUserDTO, TestUserDetails>(testUserServiceFactory);
export const useRemoveTestUser = ComposableFactory.remove(testUserServiceFactory);