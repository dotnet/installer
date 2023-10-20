#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Dockerfile that creates a container suitable to build dotnet-cli
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:fedora-36-20220716171953-531d246

RUN dnf install -y nss

RUN dnf clean all

# Override RID set by the dotnet-buildtools-prereqs docker image
ENV __PUBLISH_RID=fedora.36-x64

# Setup User to match Host User, and give superuser permissions 
ARG USER_ID=0 
RUN useradd -m code_executor -u ${USER_ID} -g wheel
RUN echo 'code_executor ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers 
 
# With the User Change, we need to change permissions on these directories 
RUN chmod -R a+rwx /usr/local 
RUN chmod -R a+rwx /home
 
# Set user to the one we just created 
USER ${USER_ID} 
 
# Set working directory 
ARG WORK_DIR 
WORKDIR ${WORK_DIR}
